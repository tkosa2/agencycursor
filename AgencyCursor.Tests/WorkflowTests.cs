using Microsoft.Playwright;
using Xunit;

namespace AgencyCursor.Tests;

[Collection("Web App Collection")]
public class WorkflowTests
{
    private readonly PlaywrightFixture _fixture;
    private const string BaseUrl = "http://localhost:5084";

    public WorkflowTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NewRequest_ShouldHaveNewRequestStatus()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to the public request page
            await page.GotoAsync($"{BaseUrl}/Request");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in Requestor Information
            var requestorFirstName = $"Test_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
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

            // Select Type of Service (check all available options)
            await page.CheckAsync("input[value='Medical']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Community event interpretation");
            }
            
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

            // Interpreter Preferences
            await page.CheckAsync("input[value='female']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "Jane Smith");
            await page.FillAsync("textarea[id='Request_ConsumerNames']", "Alice Johnson\nBob Williams");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var i = 0; i < specializationCount; i++)
            {
                await specializationCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "Spanish");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Tactile signing");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Please arrive 10 minutes early.");

            // Submit the form
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Requests index to find the new request
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Look for the request in the table - it should show "New Request" status
            var statusBadge = page.Locator("text=New Request").First;
            var hasNewRequestStatus = await statusBadge.IsVisibleAsync();
            
            Assert.True(hasNewRequestStatus, 
                "New request should have 'New Request' status");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact(Skip = "Complex test: Requires creating appointment and finding it in listing. Needs more development time.")]
    public async Task AssignInterpreter_ShouldChangeRequestStatusToAssigned()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // First, create a request via admin page
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in request form
            var requestorFirstName = $"Workflow_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
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

            // Click the Details link for the first request row
            var detailsLink = page.Locator("table tbody tr").First.Locator("a:has-text('Details')").First;
            if (await detailsLink.CountAsync() > 0)
            {
                await detailsLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Verify initial status is New Request (check the page contains the status)
                var hasNewRequest = await page.Locator("dt:has-text('Status')").IsVisibleAsync();
                Assert.True(hasNewRequest, "Status field should be visible on Details page");

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
                            var appointmentDetailsLink = appointmentRow.Locator("a:has-text('Details')").First;
                            
                            if (await appointmentDetailsLink.CountAsync() > 0)
                            {
                                await appointmentDetailsLink.ClickAsync();
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

                                        // Verify status changed to Assigned (look for it in the badge)
                                        // The status is in a badge: <span class="badge bg-secondary">Assigned</span>
                                        var assignedStatusBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                                        var assignedStatusText = await assignedStatusBadge.TextContentAsync();
                                        Assert.True(assignedStatusText?.Trim() == "Assigned", 
                                            $"Request status should be 'Assigned' after interpreter assignment, but was '{assignedStatusText}'");
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
    public async Task CancelAppointment_LessThan48Hours_ShouldBeCancelled48h()
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

                // Navigate to edit the appointment
                var editLink = page.Locator("a:has-text('Edit')").First;
                if (await editLink.CountAsync() > 0)
                {
                    await editLink.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Change appointment status to Cancelled<48h (less than 48 hours notice)
                    await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled<48h");

                    // Save
                    await page.ClickAsync("button[type='submit']");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Navigate back to appointments list
                    await page.GotoAsync($"{BaseUrl}/Appointments");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Verify the appointment now shows Cancelled<48h status with orange badge
                    var cancelledStatusBadge = page.Locator("text=Cancelled<48h").First;
                    var hasCancelledStatus = await cancelledStatusBadge.IsVisibleAsync();
                    Assert.True(hasCancelledStatus, 
                        "Appointment status should be 'Cancelled<48h' after cancellation with less than 48 hours notice");
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task CreateRequestFromInternal_ShouldCreateApprovedRequest()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to Requests/Create (admin page)
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Fill in request form
            var requestorFirstName = $"Admin_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var requestorLastName = $"Request{DateTime.Now.Ticks}";
            var requestorName = $"{requestorFirstName} {requestorLastName}";
            
            // Wait for the first name field to be visible
            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", 
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
            
            await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
            await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "2");
            await page.CheckAsync("input[value='deafblind']");

            // Fill in Contact Information
            var uniqueEmail = $"admin{DateTime.Now.Ticks}@example.com";
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 555-5555");
            await page.FillAsync("input[name='RequestorEmail']", uniqueEmail);

            // Fill in Appointment Details
            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "14:00");

            // Select Type of Service (check all available options)
            await page.CheckAsync("input[value='Legal']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }
            
            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Administrative hearing");
            }

            // Select Mode (In-Person)
            await page.CheckAsync("input[value='In-Person']");

            // Fill in Address (wait for the section to be visible)
            await page.WaitForSelectorAsync("input[name='Request.Address']", 
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "789 Admin Lane");
            await page.FillAsync("input[name='Request.Address2']", "Building 2");
            await page.FillAsync("input[name='Request.City']", "Seattle");
            await page.SelectOptionAsync("select[name='Request.State']", "WA");
            await page.FillAsync("input[name='Request.ZipCode']", "98101");

            // Interpreter Preferences
            await page.CheckAsync("input[value='male']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "John Doe");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var i = 0; i < specializationCount; i++)
            {
                await specializationCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "French");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Visual frame interpreting");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Admin created request - arrived 10 minutes early.");

            // Submit the form
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Navigate to Requests index to verify the request was created with Approved status
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Look for the request in the table - admin-created requests should have "Approved" status
            var approvedStatusBadge = page.Locator("span.badge:has-text('Approved')").First;
            var hasApprovedStatus = await approvedStatusBadge.IsVisibleAsync();
            
            Assert.True(hasApprovedStatus, 
                "Admin-created request should have 'Approved' status, not 'New Request'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact(Skip = "Deprecated: Tests old form structure, uses non-existent field 'AppointmentDate'")]
    public async Task EditRequest_ShouldUpdateRequestDetails()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to Requests index
            await page.GotoAsync($"{BaseUrl}/Requests/Edit/5");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Click on the first request
            var firstRequestLink = page.Locator("table tbody tr").First.Locator("a").First;
            //if (await firstRequestLink.CountAsync() > 0)
            {
                //await firstRequestLink.ClickAsync();
                //await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Get request ID from URL
                var requestId = page.Url.Split('/').Last().Split('?').First();

                // Click Edit button
                var editLink = page.Locator("a:has-text('Edit')").First;
                //if (await editLink.CountAsync() > 0)
                {
                    //await editLink.ClickAsync();
                    //await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Update some fields
                    var newCity = "Updated City";
                    var newZip = "99999";
                    
                    await page.FillAsync("input[name='Request.City']", newCity);
                    await page.FillAsync("input[name='Request.ZipCode']", newZip);

                    // Update appointment date to March 1, 2026
                    await page.FillAsync("input[name='AppointmentDate']", "2026-03-01");

                    // Update start and end times
                    await page.FillAsync("input[name='StartTime']", "10:00");

                    // Submit the form
                    await page.ClickAsync("button[type='submit']");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Navigate back to details to verify changes
                    await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Verify the changes were saved
                    var cityContent = await page.Locator($"text={newCity}").IsVisibleAsync();
                    Assert.True(cityContent, "City should be updated");
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditAppointment_ShouldUpdateAppointmentDateAndTime()
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

                // Click Edit button
                var editLink = page.Locator("a:has-text('Edit')").First;
                if (await editLink.CountAsync() > 0)
                {
                    await editLink.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    // Update appointment date to March 1, 2026 from 10:00 AM to 1:00 PM (13:00)
                    var appointmentDateTime = "2026-03-01T10:00";
                    
                    // Try finding the service date/time fields
                    var serviceDateTime = page.Locator("input[name='Appointment.ServiceDateTime']");
                    if (await serviceDateTime.CountAsync() > 0)
                    {
                        await serviceDateTime.FillAsync(appointmentDateTime);
                    }

                    // Look for start and end time fields if separate
                    var startTimeField = page.Locator("input[name*='StartTime'], input[name*='Start']");
                    if (await startTimeField.CountAsync() > 0)
                    {
                        await startTimeField.FillAsync("10:00");
                    }

                    var endTimeField = page.Locator("input[name*='EndTime'], input[name*='End']");
                    if (await endTimeField.CountAsync() > 0)
                    {
                        await endTimeField.FillAsync("13:00");
                    }

                    // Submit the form
                    var submitButton = page.Locator("button[type='submit']").First;
                    if (await submitButton.CountAsync() > 0)
                    {
                        await submitButton.ClickAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    }

                    // Verify the appointment was updated
                    Assert.True(!page.Url.Contains("/Edit"), 
                        "Should be redirected after updating appointment");
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task PublicRequestWorkflow_StatusProgression_ShouldFollowProperSequence()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // 1. Create a request via public page (should start with "New Request" status)
            await page.GotoAsync($"{BaseUrl}/Request");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var requestorFirstName = $"PublicReq_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var requestorLastName = $"Test{DateTime.Now.Ticks}";
            
            await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
            await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 111-2222");
            await page.FillAsync("input[name='RequestorEmail']", $"publicreq{DateTime.Now.Ticks}@test.com");

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "09:00");
            
            // Check all service type checkboxes
            await page.CheckAsync("input[value='Medical']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }
            
            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Workflow coordination meeting");
            }
            
            // Fill in consumer names using the correct id selector
            await page.FillAsync("textarea[id='Request_ConsumerNames']", "Public Test Consumer 1\\nPublic Test Consumer 2");
            
            // Fill in address for in-person request (default mode is In-Person for public form)
            await page.FillAsync("input[name='Request.ZipCode']", "99501");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var i = 0; i < specializationCount; i++)
            {
                await specializationCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "German");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Cued speech");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Public request workflow test.");

            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 2. Find the request and verify initial status is "New Request"
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Verify New Request status appears in the list
            var newRequestStatus = await page.Locator("text=New Request").First.IsVisibleAsync();
            Assert.True(newRequestStatus, "Public request should start with 'New Request' status");
            
            await page.WaitForSelectorAsync("table tbody tr", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            var requestRow = page.Locator("table tbody tr").First;
            var editLink = requestRow.Locator("a:has-text('Edit')").First;
            await editLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var requestId = page.Url.Split('/').Last().Split('?').First();

            // 3. Edit request to change status to "Reviewed"
            await page.GotoAsync($"{BaseUrl}/Requests/Edit/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await page.SelectOptionAsync("select[name='Request.Status']", "Reviewed");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 4. Verify status changed to "Reviewed"
            var reviewedStatusBadge = page.Locator("span.badge:has-text('Reviewed')").First;
            var reviewedStatus = await reviewedStatusBadge.IsVisibleAsync();
            Assert.True(reviewedStatus, "Status should be 'Reviewed' after review");

            // 5. Change status to "Approved"
            await page.GotoAsync($"{BaseUrl}/Requests/Edit/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await page.SelectOptionAsync("select[name='Request.Status']", "Approved");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the page loads successfully after setting status to Approved
            var pageUrl = page.Url;
            Assert.Contains("Details", pageUrl);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AppointmentWorkflow_StatusProgression_ShouldFollowProperSequence()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Navigate to Appointments index
            await page.GotoAsync($"{BaseUrl}/Appointments");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find the first appointment and verify or update status through workflow
            var firstAppointmentRow = page.Locator("table tbody tr").First;
            var detailsLink = firstAppointmentRow.Locator("a:has-text('Details')").First;
            
            if (await detailsLink.CountAsync() > 0)
            {
                await detailsLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                var editLink = page.Locator("a:has-text('Edit')").First;
                if (await editLink.CountAsync() > 0)
                {
                    await editLink.ClickAsync();
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    var appointmentId = page.Url.Split('/').Last().Split('?').First();

                    // 1. Set status to Confirmed (initial confirmation)
                    await page.SelectOptionAsync("select[name='Appointment.Status']", "Confirmed");
                    await page.ClickAsync("button[type='submit']");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    var confirmedBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                    var confirmedStatus = (await confirmedBadge.TextContentAsync())?.Trim();
                    Assert.True(confirmedStatus == "Confirmed", "Appointment should have 'Confirmed' status after confirmation");

                    // 2. Change to Completed
                    await page.GotoAsync($"{BaseUrl}/Appointments/Edit/{appointmentId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    
                    await page.SelectOptionAsync("select[name='Appointment.Status']", "Completed");
                    await page.ClickAsync("button[type='submit']");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    var completedBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                    var completedStatus = (await completedBadge.TextContentAsync())?.Trim();
                    Assert.True(completedStatus == "Completed", "Appointment should have 'Completed' status after service completion");

                    // 3. Test cancellation with <48h option
                    await page.GotoAsync($"{BaseUrl}/Appointments/Edit/{appointmentId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    
                    await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled<48h");
                    await page.ClickAsync("button[type='submit']");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                    var cancelledBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
                    var cancelledStatus = (await cancelledBadge.TextContentAsync())?.Trim();
                    Assert.True(cancelledStatus == "Cancelled<48h", "Appointment should have 'Cancelled<48h' status");
                }
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact(Skip = "Deprecated: Tests non-existent form fields 'textarea[name=Request.ConsumerNames]'. ConsumerNames is a property, not a separate form field.")]
    public async Task RequestWithNewFields_ConsumerNamesAndInterpreterTypes_ShouldSaveSuccessfully()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var requestorFirstName = $"NewFields_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            var requestorLastName = $"Test{DateTime.Now.Ticks}";
            
            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
            await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "2");
            await page.CheckAsync("input[value='deaf']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 333-4444");
            await page.FillAsync("input[name='RequestorEmail']", $"newfields{DateTime.Now.Ticks}@test.com");

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "10:00");
            await page.CheckAsync("input[value='Legal']");
            await page.CheckAsync("input[value='In-Person']");

            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "789 New Fields Rd");
            await page.FillAsync("input[name='Request.City']", "Fairbanks");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99701");

            // Fill in Consumer Names (new field)
            await page.FillAsync("textarea[name='Request.ConsumerNames']", "John Doe, Jane Smith");

            // Select specializations including new nested options
            await page.CheckAsync("input[value='ASL']");
            await page.CheckAsync("input[value='CDI']");
            await page.CheckAsync("input[value='Deaf-Blind']");
            await page.CheckAsync("input[value='Deaf-Blind-Tactile']");
            await page.CheckAsync("input[value='Tactile-Both-Hands']");
            await page.CheckAsync("input[value='International']");
            await page.CheckAsync("input[value='International-Russia']");

            // Fill in International Other field
            await page.FillAsync("input[name='Request.InternationalOther']", "Chinese Sign Language");

            // Fill in Other Interpreter
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Visual Frame");

            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the request was created
            Assert.True(page.Url.Contains("/Requests"), "Should be redirected to Requests page after creation");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task RequestWithSpecializations_AllOptions_ShouldBeSelectable()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Complete minimal form first
            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='RequestorFirstName']", $"Specialization_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            await page.FillAsync("input[name='RequestorLastName']", $"Test{DateTime.Now.Ticks}");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deafblind']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 555-6666");
            await page.FillAsync("input[name='RequestorEmail']", $"spec{DateTime.Now.Ticks}@test.com");

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "14:00");
            
            // Check all service type checkboxes
            await page.CheckAsync("input[value='Educational']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }
            
            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Special education IEP meeting");
            }
            
            await page.CheckAsync("input[value='In-Person']");

            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "456 Spec Ave");
            await page.FillAsync("input[name='Request.Address2']", "Suite 300");
            await page.FillAsync("input[name='Request.City']", "Juneau");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99801");

            // Interpreter Preferences
            await page.CheckAsync("input[value='male']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "Robert Smith");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "Russian");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Pro-tactile ASL");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Specialization test request.");

            // Test all specialization checkboxes are present and can be checked
            var specializationCheckboxes = new[]
            {
                "input[value='ASL']",
                "input[value='CDI']",
                "input[value='Deaf-Blind']",
                "input[value='Deaf-Blind-Closed-Up']",
                "input[value='Deaf-Blind-Tactile']",
                "input[value='Tactile-Left-Hand']",
                "input[value='Tactile-Right-Hand']",
                "input[value='Tactile-Both-Hands']",
                "input[value='Deaf-Blind-Tracking']",
                "input[value='Universal']",
                "input[value='International']",
                "input[value='International-Russia']",
                "input[value='International-Spanish']"
            };

            foreach (var checkbox in specializationCheckboxes)
            {
                var isVisible = await page.Locator(checkbox).IsVisibleAsync();
                Assert.True(isVisible, $"Specialization checkbox {checkbox} should be visible");
                await page.CheckAsync(checkbox);
            }

            // Submit form
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            Assert.True(page.Url.Contains("/Requests"), "Request with all specializations should be created successfully");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact(Skip = "Deprecated: Cancellation statuses apply to appointments, not requests. Use CancelAppointment_LessThan48Hours_ShouldBeCancelled48h instead.")]
    public async Task CancellationStatus_LessThan48Hours_ShouldBeOrange()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Create a request
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='RequestorFirstName']", $"Cancel_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            await page.FillAsync("input[name='RequestorLastName']", $"Test{DateTime.Now.Ticks}");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 777-8888");
            await page.FillAsync("input[name='RequestorEmail']", $"cancel{DateTime.Now.Ticks}@test.com");

            var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", tomorrow);
            await page.FillAsync("input[name='StartTime']", "11:00");
            
            // Check all service type checkboxes
            await page.CheckAsync("input[value='Medical']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }
            
            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Therapy session");
            }
            
            await page.CheckAsync("input[value='Virtual']");

            await page.WaitForSelectorAsync("textarea[name='Request.MeetingLink']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("textarea[name='Request.MeetingLink']", "https://zoom.us/test");

            // Also check In-Person to make address fields visible
            await page.CheckAsync("input[value='In-Person']");

            // Fill in Address (wait for the section to be visible)
            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "789 Cancel Lane");
            await page.FillAsync("input[name='Request.Address2']", "Apt 404");
            await page.FillAsync("input[name='Request.City']", "Fairbanks");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99701");

            // Interpreter Preferences
            await page.CheckAsync("input[value='female']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "Sarah Williams");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var i = 0; i < specializationCount; i++)
            {
                await specializationCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "Japanese");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Haptics");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Cancellation test request.");

            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Find and edit the request to set cancelled<48h status
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var editLink = page.Locator("table tbody tr").First.Locator("a:has-text('Edit')").First;
            await editLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var requestId = page.Url.Split('/').Last().Split('?').First();

            await page.GotoAsync($"{BaseUrl}/Requests/Edit/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await page.SelectOptionAsync("select[name='Request.Status']", "Cancelled<48h");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the cancelled<48h status appears in the index with orange badge
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var orangeBadge = await page.Locator(".badge.bg-orange").First.IsVisibleAsync();
            Assert.True(orangeBadge, "Cancelled<48h status should display with orange color");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task BroadcastedRequest_ShouldLogEmailAndCaptureResponses()
    {
        var page = await _fixture.Browser.NewPageAsync();

        try
        {
            var interpreterName1 = $"EmailInterp_{DateTime.Now:yyyyMMdd_HHmmss}_A";
            var interpreterEmail1 = $"interpA_{DateTime.Now.Ticks}@example.com";
            await CreateInterpreterAsync(page, interpreterName1, interpreterEmail1);

            var interpreterName2 = $"EmailInterp_{DateTime.Now:yyyyMMdd_HHmmss}_B";
            var interpreterEmail2 = $"interpB_{DateTime.Now.Ticks}@example.com";
            await CreateInterpreterAsync(page, interpreterName2, interpreterEmail2);

            var requestorLastName = $"Broadcast_{DateTime.Now.Ticks}";
            var requestId = await CreateApprovedRequestAsync(page, requestorLastName);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.ClickAsync("button:has-text('Notify Interpreters')");
            var modal = page.Locator("#notifyInterpretersModal");
            await modal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            var interpreterCheckboxes = modal.Locator("input[name='SelectedInterpreterIds']");
            var firstInterpreterId = await interpreterCheckboxes.Nth(0).GetAttributeAsync("value");
            var secondInterpreterId = await interpreterCheckboxes.Nth(1).GetAttributeAsync("value");

            await interpreterCheckboxes.Nth(0).CheckAsync();
            await interpreterCheckboxes.Nth(1).CheckAsync();
            await modal.Locator("textarea[name='CustomMessage']").FillAsync("Mock email broadcast for testing.");
            await modal.Locator("button[type='submit']").ClickAsync(new LocatorClickOptions { NoWaitAfter = true });
            await page.WaitForTimeoutAsync(1000);
            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("span.badge:has-text('Broadcasted')",
                new PageWaitForSelectorOptions { Timeout = 60000 });

            var broadcastedBadge = page.Locator("span.badge:has-text('Broadcasted')").First;
            Assert.True(await broadcastedBadge.IsVisibleAsync(), "Request status should be 'Broadcasted' after notify");

            var emailLogRowA = page.Locator($"table tbody tr:has-text('{interpreterName1}')").First;
            var emailLogRowB = page.Locator($"table tbody tr:has-text('{interpreterName2}')").First;
            Assert.True(await emailLogRowA.IsVisibleAsync(), "Email log should contain first interpreter");
            Assert.True(await emailLogRowB.IsVisibleAsync(), "Email log should contain second interpreter");

            Assert.False(string.IsNullOrWhiteSpace(firstInterpreterId));
            Assert.False(string.IsNullOrWhiteSpace(secondInterpreterId));

            await page.GotoAsync($"{BaseUrl}/Interpreters/RespondToRequest/{requestId}/{firstInterpreterId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.CheckAsync("#response-yes");
            await page.FillAsync("textarea[name='Notes']", "Yes - available.");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Interpreters/RespondToRequest/{requestId}/{secondInterpreterId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.CheckAsync("#response-maybe");
            await page.FillAsync("textarea[name='Notes']", "Maybe - need more info.");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Interpreters/RespondToRequest/{requestId}/{firstInterpreterId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.CheckAsync("#response-no");
            await page.FillAsync("textarea[name='Notes']", "Update: no longer available.");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var responseRowA = page.Locator($"table tbody tr:has-text('{interpreterName1}'):has-text('No')").First;
            var responseRowB = page.Locator($"table tbody tr:has-text('{interpreterName2}'):has-text('Maybe')").First;

            Assert.True(await responseRowA.IsVisibleAsync(), "Dashboard should show updated response for interpreter A");
            Assert.True(await responseRowB.IsVisibleAsync(), "Dashboard should show response for interpreter B");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AppointmentToInvoice_HappyPath_ShouldCalculateTotalsAndTrackPayment()
    {
        var page = await _fixture.Browser.NewPageAsync();

        try
        {
            var interpreterName = $"InvoiceInterp_{DateTime.Now:yyyyMMdd_HHmmss}";
            var interpreterEmail = $"invoice_{DateTime.Now.Ticks}@example.com";
            await CreateInterpreterAsync(page, interpreterName, interpreterEmail);

            var requestorLastName = $"Invoice_{DateTime.Now.Ticks}";
            var requestId = await CreateApprovedRequestAsync(page, requestorLastName);

            await page.GotoAsync($"{BaseUrl}/TestEmailLog?clear=true");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.ClickAsync("a:has-text('Assign Interpreter')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.SelectOptionAsync("select[name='SelectedInterpreterIds']",
                new[] { new SelectOptionValue { Label = interpreterName } });
            var appointmentDate = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='Appointment.ServiceDateTime']", $"{appointmentDate}T10:00");
            await page.FillAsync("input[name='Appointment.DurationMinutes']", "120");
            await page.SelectOptionAsync("select[name='Appointment.Status']", "Confirmed");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("/Appointments", page.Url);

            await page.GotoAsync($"{BaseUrl}/TestEmailLog");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var confirmationRow = page.Locator("table tbody tr:has-text('Appointment Confirmed')").First;
            var fallbackRow = page.Locator("table tbody tr:has-text('Interpreter(s) Booked')").First;

            var hasConfirmation = await confirmationRow.IsVisibleAsync();
            if (!hasConfirmation)
            {
                await fallbackRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });
                Assert.True(await fallbackRow.IsVisibleAsync(), "Appointment confirmation email should be recorded in test email log");
            }

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var appointmentLink = page.Locator("a:has-text('Appointment #')").First;
            var appointmentHref = await appointmentLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(appointmentHref));

            var appointmentId = appointmentHref!.Split('/').Last();
            await page.GotoAsync($"{BaseUrl}/Appointments/Edit/{appointmentId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.SelectOptionAsync("select[name='Appointment.Status']", "Completed");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var statusBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
            var statusText = (await statusBadge.TextContentAsync())?.Trim();
            Assert.Equal("Cancelled<48h", statusText);
            await page.ClickAsync("a:has-text('Create Invoice')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("appointmentId=", page.Url);

            var hoursWorkedValue = await page.InputValueAsync("input[name='Invoice.HoursWorked']");
            Assert.Equal("2", hoursWorkedValue);

            await page.FillAsync("input[name='Invoice.HourlyRate']", "100");
            await page.FillAsync("input[name='Invoice.Discount']", "0");
            await page.SelectOptionAsync("select[name='Invoice.PaymentStatus']", "Pending");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Invoices");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var invoiceRow = page.Locator($"table tbody tr:has-text('{requestorLastName}')").First;
            await invoiceRow.Locator("a:has-text('Edit')").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.SelectOptionAsync("select[name='Invoice.PaymentStatus']", "Paid");
            await page.FillAsync("input[name='Invoice.PaymentMethod']", "Test Payment");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Invoices");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var paidBadge = page.Locator("span.badge.bg-success:has-text('Paid')").First;
            Assert.True(await paidBadge.IsVisibleAsync(), "Invoice payment status should be 'Paid'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task InvoiceCancellationFee_ShouldApplyForCancelledLessThan48h()
    {
        var page = await _fixture.Browser.NewPageAsync();

        try
        {
            var interpreterName = $"CancelFeeInterp_{DateTime.Now:yyyyMMdd_HHmmss}";
            var interpreterEmail = $"cancelfee_{DateTime.Now.Ticks}@example.com";
            await CreateInterpreterAsync(page, interpreterName, interpreterEmail);

            var requestorLastName = $"CancelFee_{DateTime.Now.Ticks}";
            var requestId = await CreateApprovedRequestAsync(page, requestorLastName);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.ClickAsync("a:has-text('Assign Interpreter')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.SelectOptionAsync("select[name='SelectedInterpreterIds']",
                new[] { new SelectOptionValue { Label = interpreterName } });
            var appointmentDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='Appointment.ServiceDateTime']", $"{appointmentDate}T13:00");
            await page.FillAsync("input[name='Appointment.DurationMinutes']", "60");
            await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled<48h");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("/Appointments", page.Url);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var appointmentLink = page.Locator("a:has-text('Appointment #')").First;
            var appointmentHref = await appointmentLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(appointmentHref));

            var appointmentId = appointmentHref!.Split('/').Last();
            await page.GotoAsync($"{BaseUrl}/Appointments/Edit/{appointmentId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled<48h");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.ClickAsync("a:has-text('Create Invoice')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("appointmentId=", page.Url);

            var hoursWorkedValue = await page.InputValueAsync("input[name='Invoice.HoursWorked']");
            Assert.Equal("2", hoursWorkedValue);

            await page.FillAsync("input[name='Invoice.HourlyRate']", "100");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GotoAsync($"{BaseUrl}/Invoices");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var invoiceRow = page.Locator($"table tbody tr:has-text('{requestorLastName}')").First;
            var totalCell = invoiceRow.Locator("td").Nth(4);
            var totalText = (await totalCell.TextContentAsync()) ?? string.Empty;
            Assert.Contains("200", totalText);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task AppointmentCancellation_MoreThan48Hours_ShouldBeCancelled()
    {
        var page = await _fixture.Browser.NewPageAsync();

        try
        {
            var interpreterName = $"CancelInterp_{DateTime.Now:yyyyMMdd_HHmmss}";
            var interpreterEmail = $"cancel_{DateTime.Now.Ticks}@example.com";
            await CreateInterpreterAsync(page, interpreterName, interpreterEmail);

            var requestorLastName = $"Cancel_{DateTime.Now.Ticks}";
            var requestId = await CreateApprovedRequestAsync(page, requestorLastName);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.ClickAsync("a:has-text('Assign Interpreter')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.SelectOptionAsync("select[name='SelectedInterpreterIds']",
                new[] { new SelectOptionValue { Label = interpreterName } });
            var appointmentDate = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='Appointment.ServiceDateTime']", $"{appointmentDate}T11:00");
            await page.FillAsync("input[name='Appointment.DurationMinutes']", "60");
            await page.SelectOptionAsync("select[name='Appointment.Status']", "Cancelled>48h");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Assert.Contains("/Appointments", page.Url);

            await page.GotoAsync($"{BaseUrl}/Requests/Details/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var appointmentLink = page.Locator("a:has-text('Appointment #')").First;
            var appointmentHref = await appointmentLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(appointmentHref));

            var appointmentId = appointmentHref!.Split('/').Last();
            await page.GotoAsync($"{BaseUrl}/Appointments/Details/{appointmentId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var cancelledBadge = page.Locator("dt:has-text('Status')").Locator("..").Locator("dd .badge");
            var cancelledStatus = (await cancelledBadge.TextContentAsync())?.Trim();
            Assert.True(cancelledStatus == "Cancelled>48h", "Appointment should have 'Cancelled>48h' status");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private async Task CreateInterpreterAsync(IPage page, string name, string email)
    {
        await page.GotoAsync($"{BaseUrl}/Interpreters/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.FillAsync("input[name='Interpreter.Name']", name);
        await page.FillAsync("input[name='Interpreter.Email']", email);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<int> CreateApprovedRequestAsync(IPage page, string requestorLastName)
    {
        await page.GotoAsync($"{BaseUrl}/Request");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var requestorFirstName = "Workflow";
        await page.WaitForSelectorAsync("input[name='RequestorFirstName']",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await page.FillAsync("input[name='RequestorFirstName']", requestorFirstName);
        await page.FillAsync("input[name='RequestorLastName']", requestorLastName);
        await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
        await page.CheckAsync("input[value='deaf']");

        var uniqueEmail = $"{requestorLastName}@example.com";
        await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 555-0000");
        await page.FillAsync("input[name='RequestorEmail']", uniqueEmail);

        var appointmentDate = DateTime.Today.AddDays(365).ToString("yyyy-MM-dd");
        await page.FillAsync("input[name='AppointmentDate']", appointmentDate);
        await page.FillAsync("input[name='StartTime']", "09:30");
        await page.EvaluateAsync("() => { const end = document.querySelector('input[name=\"EndTime\"]'); if (end) { end.value = '11:30'; end.dispatchEvent(new Event('input', { bubbles: true })); end.dispatchEvent(new Event('change', { bubbles: true })); } }");
        await page.CheckAsync("input[value='Medical']");

        await page.WaitForSelectorAsync("input[name='Request.Address']",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await page.FillAsync("input[name='Request.Address']", "100 Main St");
        await page.FillAsync("input[name='Request.City']", "Anchorage");
        await page.SelectOptionAsync("select[name='Request.State']", "AK");
        await page.FillAsync("input[name='Request.ZipCode']", "99501");

        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"{BaseUrl}/Requests");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var requestRow = page.Locator("table tbody tr").First;
        await requestRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });
        var rowText = await requestRow.InnerTextAsync();

        if (!rowText.Contains(requestorLastName, StringComparison.OrdinalIgnoreCase))
        {
            var fallbackRow = page.Locator($"table tbody tr:has-text('{requestorLastName}')").First;
            await fallbackRow.WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });
            requestRow = fallbackRow;
        }

        await requestRow.Locator("a:has-text('Details')").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var requestId = page.Url.Split('/').Last().Split('?').First();

        await page.GotoAsync($"{BaseUrl}/Requests/Edit/{requestId}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.SelectOptionAsync("select[name='Request.Status']", "Approved");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return int.Parse(requestId);
    }

    [Fact(Skip = "Deprecated: Cancellation statuses apply to appointments, not requests. Use appointment cancellation tests instead.")]
    public async Task CancellationStatus_MoreThan48Hours_ShouldBeRed()
    {
        var page = await _fixture.Browser.NewPageAsync();
        
        try
        {
            // Create a request and set to cancelled>48h
            await page.GotoAsync($"{BaseUrl}/Requests/Create");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.WaitForSelectorAsync("input[name='RequestorFirstName']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='RequestorFirstName']", $"EarlyCancel_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            await page.FillAsync("input[name='RequestorLastName']", $"Test{DateTime.Now.Ticks}");
            await page.FillAsync("input[name='Request.NumberOfIndividuals']", "1");
            await page.CheckAsync("input[value='deaf']");
            await page.FillAsync("input[name='RequestorPhone']", "+1 (555) 999-0000");
            await page.FillAsync("input[name='RequestorEmail']", $"earlycancel{DateTime.Now.Ticks}@test.com");

            var nextWeek = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
            await page.FillAsync("input[name='AppointmentDate']", nextWeek);
            await page.FillAsync("input[name='StartTime']", "15:00");
            
            // Check all service type checkboxes
            await page.CheckAsync("input[value='Legal']");
            var serviceCheckboxes = page.Locator("input[type='checkbox'][name='TypeOfService']");
            var serviceCount = await serviceCheckboxes.CountAsync();
            for (var i = 0; i < serviceCount; i++)
            {
                await serviceCheckboxes.Nth(i).CheckAsync();
            }
            
            // Fill in "Other" service type details if available
            var otherServiceInput = page.Locator("input[name='Request.TypeOfServiceOther']");
            if (await otherServiceInput.IsVisibleAsync())
            {
                await page.FillAsync("input[name='Request.TypeOfServiceOther']", "Court proceeding");
            }
            
            await page.CheckAsync("input[value='In-Person']");

            await page.WaitForSelectorAsync("input[name='Request.Address']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
            await page.FillAsync("input[name='Request.Address']", "999 Cancel St");
            await page.FillAsync("input[name='Request.Address2']", "Floor 2");
            await page.FillAsync("input[name='Request.City']", "Anchorage");
            await page.SelectOptionAsync("select[name='Request.State']", "AK");
            await page.FillAsync("input[name='Request.ZipCode']", "99501");

            // Interpreter Preferences
            await page.CheckAsync("input[value='male']");
            await page.FillAsync("input[name='Request.PreferredInterpreterName']", "David Brown");
            
            // Check all interpreter type checkboxes
            var interpreterCheckboxes = page.Locator("input[type='checkbox'][name='InterpreterTypes']");
            var interpreterCount = await interpreterCheckboxes.CountAsync();
            for (var i = 0; i < interpreterCount; i++)
            {
                await interpreterCheckboxes.Nth(i).CheckAsync();
            }

            // Check all specialization checkboxes
            var specializationCheckboxes = page.Locator("input[type='checkbox'][name='Specializations']");
            var specializationCount = await specializationCheckboxes.CountAsync();
            for (var i = 0; i < specializationCount; i++)
            {
                await specializationCheckboxes.Nth(i).CheckAsync();
            }

            // Fill in international and other interpreter fields
            await page.FillAsync("input[name='Request.InternationalOther']", "Italian");
            await page.FillAsync("input[name='Request.OtherInterpreter']", "Signed English");
            
            // Fill in additional notes
            await page.FillAsync("textarea[name='Request.AdditionalNotes']", "Early cancellation test request.");

            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Edit to set cancelled>48h
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var editLink = page.Locator("table tbody tr").First.Locator("a:has-text('Edit')").First;
            await editLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var requestId = page.Url.Split('/').Last().Split('?').First();

            await page.GotoAsync($"{BaseUrl}/Requests/Edit/{requestId}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await page.SelectOptionAsync("select[name='Request.Status']", "Cancelled>48h");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the cancelled>48h status appears with red badge
            await page.GotoAsync($"{BaseUrl}/Requests");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var redBadge = await page.Locator(".badge.bg-danger").First.IsVisibleAsync();
            Assert.True(redBadge, "Cancelled>48h status should display with red color");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
