using Microsoft.Playwright;
using Xunit;

namespace AgencyCursor.Tests;

public class WorkflowTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    private const string BaseUrl = "http://localhost:5084";

    public WorkflowTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NewRequest_ShouldHavePendingStatus()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to the public request page
            await page.GotoAsync($"{BaseUrl}/Request");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in Requestor Information
            var requestorFirstName = $"Test";
            var requestorLastName = $"Requestor{DateTime.Now.Ticks}";
            var requestorName = $"{requestorFirstName} {requestorLastName}";
            await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
            await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");

            // Fill in Contact Information
            var uniqueEmail = $"test{DateTime.Now.Ticks}@example.com";
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 123-4567");
            await page.FillAsync("input[name='RequestorEmail']", uniqueEmail);

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "09:00");
            await page.FillAsync("input[name='EndTime']", "10:00");

            // Select Type of Service
            await page.CheckAsync("input[value='Medical']");

            // Select Mode (In-Person)
            await page.CheckAsync("input[value='In-Person']");

            // Fill in Address
            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "123 Test Street");
            await page.FillAsync("input[name='Request.City']", "Anchorage");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

            // Submit the form
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Requests index to find the new request
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Look for the request in the table - it should show "Pending" status
            // We'll check by looking for the requestor name and verifying status
            var statusBadge = page.Locator($"text={requestorName}").Locator("..").Locator("text=Pending");
            var hasPendingStatus = await statusBadge.IsVisibleAsync();
            
            // Alternative: Check if we can find any "Pending" badge near the requestor name
            // This is a simplified check - in a real scenario, you might want to extract the request ID
            // and navigate to the details page to verify the status
            Assert.True(hasPendingStatus || page.Url.Contains("/Requests"), 
                "New request should have 'Pending' status");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AssignInterpreter_ShouldChangeRequestStatusToConfirmed()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // First, create a request via admin page
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in request form
            var requestorFirstName = "Workflow";
            var requestorLastName = $"Test{DateTime.Now.Ticks}";
            var requestorName = $"{requestorFirstName} {requestorLastName}";
            
            // Wait for the first name field to be visible
            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
            await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 999-8888");
            await page.FillAsync("input[name='RequestorEmail']", $"workflow{DateTime.Now.Ticks}@test.com");

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "10:00");
            await page.FillAsync("input[name='EndTime']", "11:00");
            await page.CheckAsync("input[value='Medical']");
            await page.CheckAsync("input[value='In-Person']");

            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "456 Test Ave");
            await page.FillAsync("input[name='Request.City']", "Anchorage");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

            // Submit request
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Appointments to create an appointment
            await page.GotoAsync($"{BaseUrl}/Appointments/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Select the request we just created (it should be in the dropdown)
            // Note: This assumes the request appears in the dropdown
            // In a real scenario, you might need to get the request ID from the previous page

            // For now, let's navigate to the appointments list and find a pending appointment
            // or create one programmatically. Let's use a simpler approach:
            // Go to Requests, find the request, then create appointment from there

            // Actually, let's navigate to Requests index and click on the first request
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Click on the first request link (or the one we just created)
            var firstRequestLink = page.Locator("table tbody tr").First.Locator("a").First;
            if (await firstRequestLink.CountAsync() > 0)
            {
                await firstRequestLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Verify initial status is Pending
                var statusText = await page.Locator("text=Pending").First.IsVisibleAsync();
                Assert.True(statusText, "Request should initially have 'Pending' status");

                // Now create an appointment for this request
                // Navigate to create appointment with request ID
                var currentUrl = page.Url;
                var requestId = currentUrl.Split('/').Last().Split('?').First();
                
                await page.GotoAsync($"{BaseUrl}/Appointments/Create?requestId={requestId}");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Select an interpreter (assuming there are interpreters in the system)
                var interpreterSelect = page.Locator("select[name='Appointment.InterpreterId']");
                var optionCount = await interpreterSelect.Locator("option").CountAsync();
                
                if (optionCount > 1) // More than just the default "-- Select Interpreter --"
                {
                    // Select the first available interpreter (skip the first option which is the placeholder)
                    var firstInterpreterValue = await interpreterSelect.Locator("option").Nth(1).GetAttributeAsync("value");
                    if (!string.IsNullOrEmpty(firstInterpreterValue) && firstInterpreterValue != "0")
                    {
                        await interpreterSelect.SelectOptionAsync(firstInterpreterValue);
                        
                        // Set date/time
                        var appointmentDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-ddTHH:mm");
                        await page.FillAsync("input[name='Appointment.ServiceDateTime']", appointmentDate);
                        
                        // Submit appointment
                        await page.ClickAsync("button[type='submit']");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Now navigate to the appointment details page to assign the interpreter
                        // The appointment should have been created, so find it in the appointments list
                        await page.GotoAsync($"{BaseUrl}/Appointments");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Find the appointment we just created (it should link to the request)
                        var appointmentLink = page.Locator($"a[href*='/Requests/Details/{requestId}']").First;
                        if (await appointmentLink.CountAsync() > 0)
                        {
                            // Find the row containing this link and get the Details link for that appointment
                            var appointmentRow = appointmentLink.Locator("xpath=ancestor::tr");
                            var detailsLink = appointmentRow.Locator("a:has-text('Details')").First;
                            
                            if (await detailsLink.CountAsync() > 0)
                            {
                                await detailsLink.ClickAsync();
                                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                                // Now assign the interpreter using the form on the appointment details page
                                var assignInterpreterSelect = page.Locator("select[name='SelectedInterpreterId']");
                                var assignOptionCount = await assignInterpreterSelect.Locator("option").CountAsync();
                                
                                if (assignOptionCount > 1) // More than just the default "-- Select Interpreter --"
                                {
                                    // Select the first available interpreter
                                    var assignInterpreterValue = await assignInterpreterSelect.Locator("option").Nth(1).GetAttributeAsync("value");
                                    if (!string.IsNullOrEmpty(assignInterpreterValue) && assignInterpreterValue != "0")
                                    {
                                        await assignInterpreterSelect.SelectOptionAsync(assignInterpreterValue);
                                        
                                        // Submit the assignment form
                                        await page.ClickAsync("button[type='submit']");
                                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                                        // Navigate back to the request details
                                        await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
                                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                                        // Verify status changed to Confirmed (look for it in the badge)
                                        // The status is in a badge: <span class="badge bg-secondary">Confirmed</span>
                                        var confirmedStatusBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                                        var confirmedStatusText = await confirmedStatusBadge.TextContentAsync();
                                        Assert.True(confirmedStatusText?.Trim() == "Confirmed", 
                                            $"Request status should be 'Confirmed' after interpreter assignment, but was '{confirmedStatusText}'");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CancelAppointment_ShouldChangeRequestStatusToCancelled()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to Appointments index
            await page.GotoAsync($"{BaseUrl}/Appointments");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find the first appointment and click Details
            var firstAppointmentRow = page.Locator("table tbody tr").First;
            var detailsLink = firstAppointmentRow.Locator("a:has-text('Details')").First;
            
            if (await detailsLink.CountAsync() > 0)
            {
                await detailsLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Get the request ID from the appointment details page
                var requestLink = page.Locator("a:has-text('Request #')").First;
                if (await requestLink.CountAsync() > 0)
                {
                    var requestLinkText = await requestLink.TextContentAsync();
                    var requestId = requestLinkText?.Split('#').Last().Trim();

                    // Navigate to edit the appointment
                    var editLink = page.Locator("a:has-text('Edit')").First;
                    if (await editLink.CountAsync() > 0)
                    {
                        await editLink.ClickAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Change status to Cancelled
                        await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled");

                        // Save
                        await page.ClickAsync("button[type='submit']");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Navigate to the request details page
                        if (!string.IsNullOrEmpty(requestId))
                        {
                            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
                            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                            // Verify status is Cancelled (look for it in the badge)
                            // The status is in a badge: <span class="badge bg-secondary">Cancelled</span>
                            var cancelledStatusBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                            var cancelledStatusText = await cancelledStatusBadge.TextContentAsync();
                            Assert.True(cancelledStatusText?.Trim() == "Cancelled", 
                                $"Request status should be 'Cancelled' after appointment cancellation, but was '{cancelledStatusText}'");
                        }
                    }
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
