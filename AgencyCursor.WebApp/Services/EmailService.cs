using System.Net;
using System.Net.Mail;
using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Hosting;

namespace AgencyCursor.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, int? requestId = null, int? interpreterId = null);
    Task SendEmailsAsync(IEnumerable<string> toEmails, string subject, string htmlBody);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly AgencyDbContext _db;

    public EmailService(IConfiguration configuration, IWebHostEnvironment environment, AgencyDbContext db)
    {
        _configuration = configuration;
        _environment = environment;
        _db = db;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, int? requestId = null, int? interpreterId = null)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var smtpHost = smtpSettings["Host"];
        var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
        var fromEmail = smtpSettings["FromEmail"];
        var fromName = smtpSettings["FromName"];
        var userName = smtpSettings["Username"];
        var password = smtpSettings["Password"];
        var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

        // During development, redirect all emails to the test email address
        var recipientEmail = toEmail;
        if (_environment.IsDevelopment())
        {
            var testEmailAddress = smtpSettings["TestEmailAddress"];
            if (!string.IsNullOrEmpty(testEmailAddress))
            {
                recipientEmail = testEmailAddress;
                fromEmail = testEmailAddress; // Override from email to avoid confusion
                fromName = "AgencyCursor (Dev)";
            }
        }

        var emailLog = new InterpreterEmailLog
        {
            RequestId = requestId ?? 0,
            InterpreterId = interpreterId ?? 0,
            SentAt = DateTime.UtcNow,
            Status = "Success"
        };

        using (var client = new SmtpClient(smtpHost, smtpPort))
        {
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(userName, password);

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                try
                {
                    await client.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    emailLog.Status = "Failed";
                    emailLog.ErrorMessage = ex.Message;
                    Console.WriteLine($"Failed to send email to {recipientEmail}: {ex.Message}");
                }
            }
        }

        // Log the email if we have request/interpreter context
        if (requestId.HasValue && interpreterId.HasValue && requestId > 0 && interpreterId > 0)
        {
            _db.InterpreterEmailLogs.Add(emailLog);
            await _db.SaveChangesAsync();
        }
    }

    public async Task SendEmailsAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var smtpHost = smtpSettings["Host"];
        var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
        var fromEmail = smtpSettings["FromEmail"];
        var fromName = smtpSettings["FromName"];
        var userName = smtpSettings["Username"];
        var password = smtpSettings["Password"];
        var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

        // During development, redirect all emails to the test email address
        IEnumerable<string> recipientEmails = toEmails;
        if (_environment.IsDevelopment())
        {
            var testEmailAddress = smtpSettings["TestEmailAddress"];
            if (!string.IsNullOrEmpty(testEmailAddress))
            {
                recipientEmails = new[] { testEmailAddress };
                fromEmail = testEmailAddress; // Override from email to avoid confusion
                fromName = "AgencyCursor (Dev)";
            }
        }

        using (var client = new SmtpClient(smtpHost, smtpPort))
        {
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(userName, password);

            foreach (var email in recipientEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmail, fromName);
                    message.To.Add(new MailAddress(email));
                    message.Subject = subject;
                    message.Body = htmlBody;
                    message.IsBodyHtml = true;

                    try
                    {
                        await client.SendMailAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
                        // Log error but continue with other emails
                    }
                }
            }
        }
    }
}
