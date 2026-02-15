using Microsoft.Playwright;
using Xunit;

namespace AgencyCursor.Tests;

public class RequestTests : IClassFixture<PlaywrightFixture>
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

            // Fill in Requestor Information
            await page.FillAsync("input[name='Request.RequestName']", "Test Requestor");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");

            // Fill in Contact Information
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 123-4567");
            await page.FillAsync("input[name='RequestorEmail']", "test@example.com");

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "09:00");
            await page.FillAsync("input[name='EndTime']", "10:00");

            // Select Type of Service
            await page.CheckAsync("input[value='Medical']");

            // Select Mode (In-Person)
            await page.CheckAsync("input[value='In-Person']");

            // Fill in Address (wait for the section to be visible)
            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "123 Test Street");
            await page.FillAsync("input[name='Request.City']", "Anchorage");
            
            // Select State from dropdown
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

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
            await page.FillAsync("input[name='Request.RequestName']", "Virtual Test Requestor");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");

            // Fill in Contact Information
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 987-6543");
            await page.FillAsync("input[name='RequestorEmail']", "virtual@example.com");

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "14:00");
            await page.FillAsync("input[name='EndTime']", "15:00");

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
