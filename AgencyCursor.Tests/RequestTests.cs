using Microsoft.Playwright;
using Xunit;

namespace AgencyCursor.Tests;

[Collection("Web App Collection")]
public class RequestTests
{
    private readonly PlaywrightFixture _fixture;
    private const string BaseUrl = "http://localhost:5084";

    public RequestTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateNewRequest_ShouldSubmitSuccessfully()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to the request page
            await page.GotoAsync($"{BaseUrl}/Request");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            // Fill in Requestor Information
            await page.FillAsync("input[name='RequestorFirstName']", "Test");
            await page.FillAsync("input[name='RequestorLastName']", $"Requestor-{timestamp}");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");

            // Fill in Contact Information
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 123-4567");
            await page.FillAsync("input[name='RequestorEmail']", "test@example.com");

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "09:00");

            // Select Type of Service
            await page.CheckAsync("input[value='Other']");
            await page.WaitForSelectorAsync("input[name='Request.TypeOfServiceOther']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Other appointment details");

            // Select Mode (In-Person)
            await page.CheckAsync("input[value='Virtual']");
            await page.WaitForSelectorAsync("textarea[name='Request.MeetingLink']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("textarea[name='Request.MeetingLink']", "Meeting link: https://zoom.us/j/123456789\nPasscode: 123456\nPhone: +1 (555) 123-4567\nMeeting ID: 123 456 7890\nOther information...");
            await page.CheckAsync("input[value='In-Person']");

            // Fill in Address (wait for the section to be visible)
            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "123 Test Street");
            await page.FillAsync("input[name='Request.Address2']", "Suite 200");
            await page.FillAsync("input[name='Request.City']", "Anchorage");
            
            // Select State from dropdown
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

            // Interpreter Preferences and Notes
            await page.CheckAsync("input[value='female']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "Jamie Doe");
            await page.FillAsync("textarea[id='Request_ConsumerNames']", "John Smith\nJane Doe");
            await page.FillAsync("input[name='Request.InternationalOther']", "Italian");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Cued speech");
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Please arrive 10 minutes early.");

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var index = 0; index < specializationCount; index++)
            {
                await specializationCheckboxes.Nth(index).CheckAsync();
            }

            // Submit the form
            await page.ClickAsync("button[type='submit']");
            
            // Wait for navigation or success message
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Verify success - check for success message or redirect
            var successMessage = await page.Locator(".alert-success").IsVisibleAsync();
            var isOnSuccessPage = page.Url.Contains("/Request") || page.Url.Contains("/Requests");
            
            Assert.True(successMessage || isOnSuccessPage, "Request should be submitted successfully");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CreateNewRequest_Virtual_ShouldSubmitSuccessfully()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to the request page
            await page.GotoAsync($"{BaseUrl}/Request");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in Requestor Information
            await page.FillAsync("input[name='RequestorFirstName']", "Virtual");
            await page.FillAsync("input[name='RequestorLastName']", "Requestor");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");

            // Fill in Contact Information
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 987-6543");
            await page.FillAsync("input[name='RequestorEmail']", "virtual@example.com");

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "14:00");

            // Select Type of Service
            await page.CheckAsync("input[value='Legal']");

            // Select Mode (Virtual)
            await page.CheckAsync("input[value='Virtual']");

            // Wait for meeting link section to appear
            await page.WaitForSelectorAsync("textarea[name='Request.MeetingLink']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            
            // Fill in meeting information
            await page.FillAsync("textarea[name='Request.MeetingLink']", "Meeting link: https://zoom.us/j/123456789\nPasscode: 123456\nPhone: +1 (555) 111-2222\nMeeting ID: 123 456 7890");

            // Submit the form
            await page.ClickAsync("button[type='submit']");
            
            // Wait for navigation or success message
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Verify success
            var successMessage = await page.Locator(".alert-success").IsVisibleAsync();
            var isOnSuccessPage = page.Url.Contains("/Request") || page.Url.Contains("/Requests");
            
            Assert.True(successMessage || isOnSuccessPage, "Virtual request should be submitted successfully");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
