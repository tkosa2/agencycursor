using AgencyCursor.Models;
using System.Text.Json;

namespace AgencyCursor.Data;

public static class MockDataGenerator
{
    public static List<Requestor> GenerateRequestors()
    {
        return new List<Requestor>
        {
            // Medical facilities
            new Requestor { Name = "Metro Health Clinic", Phone = "+1 (555) 200-1000", Email = "scheduling@metrohealth.example.com", Address = "500 Hospital Dr, Anchorage, AK 99501", Notes = "Medical appointments; prefer ASL-certified interpreters." },
            new Requestor { Name = "Alaska Regional Hospital", Phone = "+1 (555) 200-1100", Email = "interpreter@alaskaregional.example.com", Address = "2801 DeBarr Rd, Anchorage, AK 99508", Notes = "Large hospital; frequent requests for medical interpreting." },
            new Requestor { Name = "Providence Medical Center", Phone = "+1 (555) 200-1200", Email = "accessibility@providence.example.com", Address = "3200 Providence Dr, Anchorage, AK 99508", Notes = "Requires CDI for complex medical cases." },
            
            // Legal services
            new Requestor { Name = "Riverside Legal Aid", Phone = "+1 (555) 200-2000", Email = "intake@riversidelegal.example.com", Address = "100 Court St, Suite 50, Anchorage, AK 99501", Notes = "Legal interpreting; often last-minute requests." },
            new Requestor { Name = "Alaska Public Defender Agency", Phone = "+1 (555) 200-2100", Email = "interpreter@apda.example.com", Address = "303 K St, Anchorage, AK 99501", Notes = "Court appearances; requires certified interpreters." },
            new Requestor { Name = "Fairbanks Law Group", Phone = "+1 (555) 200-2200", Email = "admin@fairbankslaw.example.com", Address = "750 W 2nd Ave, Fairbanks, AK 99701", Notes = "Civil law cases; prefers interpreters with legal experience." },
            
            // Educational institutions
            new Requestor { Name = "University of Alaska Anchorage", Phone = "+1 (555) 200-3000", Email = "disability@uaa.example.com", Address = "3211 Providence Dr, Anchorage, AK 99508", Notes = "Educational interpreting for classes and events." },
            new Requestor { Name = "Anchorage School District", Phone = "+1 (555) 200-3100", Email = "specialed@asd.example.com", Address = "5530 E Northern Lights Blvd, Anchorage, AK 99504", Notes = "K-12 educational interpreting services." },
            
            // Individual clients
            new Requestor { Name = "John Doe", Phone = "+1 (555) 123-4567", Email = "johndoe@example.com", Address = "123 Main St, Suite 101, Anchorage, AK 99501", Notes = "Individual client; medical appointments." },
            new Requestor { Name = "Sarah Johnson", Phone = "+1 (555) 123-4568", Email = "sarah.johnson@example.com", Address = "456 Elm Ave, Fairbanks, AK 99701", Notes = "Deaf-Blind individual; requires tactile interpreting." },
            new Requestor { Name = "Michael Chen", Phone = "+1 (555) 123-4569", Email = "mchen@example.com", Address = "789 Oak St, Juneau, AK 99801", Notes = "Frequent client; prefers same interpreter when possible." },
            new Requestor { Name = "Emily Rodriguez", Phone = "+1 (555) 123-4570", Email = "emily.r@example.com", Address = "321 Pine Rd, Anchorage, AK 99503", Notes = "Legal case; needs interpreter for court dates." },
            
            // Government agencies
            new Requestor { Name = "Alaska Department of Health", Phone = "+1 (555) 200-4000", Email = "accessibility@alaskahealth.example.com", Address = "3601 C St, Anchorage, AK 99503", Notes = "State agency; various appointment types." },
            new Requestor { Name = "Social Security Administration", Phone = "+1 (555) 200-4100", Email = "interpreter@ssa.example.com", Address = "701 C St, Anchorage, AK 99501", Notes = "Federal agency; disability hearings." }
        };
    }

    public static List<Interpreter> GenerateRidImportedInterpreters()
    {
        var interpreters = new List<Interpreter>();
        var random = new Random();
        
        // Sample RID-style data structures
        var ridDataSamples = new[]
        {
            new Dictionary<string, object?>
            {
                ["FirstName"] = "Sarah",
                ["LastName"] = "Johnson",
                ["Email"] = "sarah.johnson@rid.example.com",
                ["Phone"] = "+1 (206) 555-0101",
                ["City"] = "Seattle",
                ["State"] = "WA",
                ["ZipCode"] = "98101",
                ["Category"] = "Certified",
                ["FreelanceStatus"] = "Yes",
                ["Gender"] = "Female",
                ["Certification"] = "NIC Advanced",
                ["Languages"] = "ASL",
                ["CertificateType"] = "NIC Advanced",
                ["MemberID"] = "RID-12345",
                ["Address"] = "123 Main St, Seattle, WA 98101"
            },
            new Dictionary<string, object?>
            {
                ["FirstName"] = "Michael",
                ["LastName"] = "Chen",
                ["Email"] = "michael.chen@rid.example.com",
                ["Phone"] = "+1 (206) 555-0102",
                ["City"] = "Tacoma",
                ["State"] = "WA",
                ["ZipCode"] = "98402",
                ["Category"] = "Certified",
                ["FreelanceStatus"] = "Yes",
                ["Gender"] = "Male",
                ["Certification"] = "CDI",
                ["Languages"] = "ASL, Tactile",
                ["CertificateType"] = "CDI",
                ["MemberID"] = "RID-12346",
                ["Address"] = "456 Oak Ave, Tacoma, WA 98402"
            },
            new Dictionary<string, object?>
            {
                ["FirstName"] = "Emily",
                ["LastName"] = "Rodriguez",
                ["Email"] = "emily.rodriguez@rid.example.com",
                ["Phone"] = "+1 (206) 555-0103",
                ["City"] = "Spokane",
                ["State"] = "WA",
                ["ZipCode"] = "99201",
                ["Category"] = "Certified",
                ["FreelanceStatus"] = "Yes",
                ["Gender"] = "Female",
                ["Certification"] = "NIC Master",
                ["Languages"] = "ASL, Spanish",
                ["CertificateType"] = "NIC Master",
                ["MemberID"] = "RID-12347",
                ["Address"] = "789 Pine St, Spokane, WA 99201"
            },
            new Dictionary<string, object?>
            {
                ["FirstName"] = "David",
                ["LastName"] = "Thompson",
                ["Email"] = "david.thompson@rid.example.com",
                ["Phone"] = "+1 (206) 555-0104",
                ["City"] = "Bellevue",
                ["State"] = "WA",
                ["ZipCode"] = "98004",
                ["Category"] = "Certified",
                ["FreelanceStatus"] = "Yes",
                ["Gender"] = "Male",
                ["Certification"] = "RSC",
                ["Languages"] = "ASL",
                ["CertificateType"] = "RSC",
                ["MemberID"] = "RID-12348",
                ["Address"] = "321 Cedar Blvd, Bellevue, WA 98004"
            },
            new Dictionary<string, object?>
            {
                ["FirstName"] = "Jennifer",
                ["LastName"] = "Martinez",
                ["Email"] = "jennifer.martinez@rid.example.com",
                ["Phone"] = "+1 (206) 555-0105",
                ["City"] = "Everett",
                ["State"] = "WA",
                ["ZipCode"] = "98201",
                ["Category"] = "Certified",
                ["FreelanceStatus"] = "Yes",
                ["Gender"] = "Female",
                ["Certification"] = "CI/CT",
                ["Languages"] = "ASL, Tactile",
                ["CertificateType"] = "CI/CT",
                ["MemberID"] = "RID-12349",
                ["Address"] = "654 Elm St, Everett, WA 98201"
            }
        };

        foreach (var ridData in ridDataSamples)
        {
            var firstName = ridData["FirstName"]?.ToString() ?? "";
            var lastName = ridData["LastName"]?.ToString() ?? "";
            var name = $"{firstName} {lastName}".Trim();
            
            var ridDataJson = JsonSerializer.Serialize(ridData);
            
            interpreters.Add(new Interpreter
            {
                Name = name,
                Email = ridData["Email"]?.ToString(),
                Phone = ridData["Phone"]?.ToString(),
                Certification = ridData["Certification"]?.ToString(),
                Languages = ridData["Languages"]?.ToString() ?? "ASL",
                Availability = "Contact for availability",
                IsRegisteredWithAgency = true,
                Notes = $"Imported from RID database. Full data: {ridDataJson}"
            });
        }

        return interpreters;
    }

    public static List<Interpreter> GenerateInterpreters()
    {
        return new List<Interpreter>
        {
            new Interpreter 
            { 
                Name = "Jane Smith", 
                Languages = "ASL (American Sign Language)", 
                Availability = "Mon–Fri 8am–5pm", 
                Phone = "+1 (555) 300-1000", 
                Email = "jane.smith@interpret.example.com", 
                Certification = "CDI, ASL-certified, RID Certified", 
                Notes = "Specializes in medical and legal interpreting. 10+ years experience.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "Carlos Mendez", 
                Languages = "Spanish, ASL", 
                Availability = "Mon–Thu 9am–4pm", 
                Phone = "+1 (555) 300-2000", 
                Email = "carlos.mendez@interpret.example.com", 
                Certification = "State certified, RID", 
                Notes = "Bilingual interpreter. Educational and medical specialties.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "Jane Doe", 
                Languages = "ASL, Tactile (Deaf-Blind)", 
                Availability = "Flexible", 
                Phone = "+1 (555) 300-3000", 
                Email = "jane.doe@interpret.example.com", 
                Certification = "CDI, Tactile, RID Certified", 
                Notes = "Specializes in Deaf-Blind interpreting. Preferred for tactile services.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "Robert Williams", 
                Languages = "ASL, Russian Sign Language", 
                Availability = "Mon–Fri 7am–6pm", 
                Phone = "+1 (555) 300-4000", 
                Email = "r.williams@interpret.example.com", 
                Certification = "RID Certified, State Licensed", 
                Notes = "Medical and legal interpreting. Fluent in RSL.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "Maria Garcia", 
                Languages = "ASL, Spanish", 
                Availability = "Tue–Sat 10am–7pm", 
                Phone = "+1 (555) 300-5000", 
                Email = "maria.garcia@interpret.example.com", 
                Certification = "CDI, RID Certified", 
                Notes = "Educational and medical interpreting. Bilingual services.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "David Kim", 
                Languages = "ASL", 
                Availability = "Mon–Wed, Fri 9am–5pm", 
                Phone = "+1 (555) 300-6000", 
                Email = "david.kim@interpret.example.com", 
                Certification = "RID Certified", 
                Notes = "Legal and court interpreting specialist.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "Lisa Anderson", 
                Languages = "ASL, Tactile, Low Vision", 
                Availability = "Mon–Fri 8am–4pm", 
                Phone = "+1 (555) 300-7000", 
                Email = "lisa.anderson@interpret.example.com", 
                Certification = "CDI, Tactile Certified, RID", 
                Notes = "Deaf-Blind specialist. Tactile and close-up interpreting.",
                IsRegisteredWithAgency = true
            },
            new Interpreter 
            { 
                Name = "James Thompson", 
                Languages = "ASL", 
                Availability = "Flexible, evenings available", 
                Phone = "+1 (555) 300-8000", 
                Email = "j.thompson@interpret.example.com", 
                Certification = "RID Certified", 
                Notes = "General interpreting. Available for last-minute requests.",
                IsRegisteredWithAgency = true
            }
        };
    }

    public static List<Request> GenerateRequests(List<Requestor> requestors, List<Interpreter> interpreters)
    {
        var random = new Random();
        var requests = new List<Request>();
        var baseDate = DateTime.Today;
        var statuses = new[] { "Pending", "Assigned", "Confirmed", "Completed", "Cancelled" };
        var serviceTypes = new[] { "Medical", "Legal", "Educational", "Other" };
        var modes = new[] { "In-Person", "Virtual" };
        var individualTypes = new[] { "deaf", "deafblind", "deaf_deafblind_hh" };
        var genderPrefs = new[] { "male", "female", "none" };
        var specializations = new[] { "ASL", "CDI", "Tactile", "Russian", "Low vision" };

        // Generate requests for each requestor
        foreach (var requestor in requestors)
        {
            var requestCount = random.Next(1, 4); // 1-3 requests per requestor
            
            for (int i = 0; i < requestCount; i++)
            {
                var daysOffset = random.Next(-30, 60); // Past 30 days to future 60 days
                var serviceDate = baseDate.AddDays(daysOffset);
                var startHour = random.Next(8, 17); // 8am to 4pm
                var startMinute = random.Next(0, 2) * 30; // 0 or 30 minutes
                var serviceDateTime = serviceDate.Date.AddHours(startHour).AddMinutes(startMinute);
                var duration = random.Next(1, 4); // 1-3 hours
                var endDateTime = serviceDateTime.AddHours(duration);

                var serviceType = serviceTypes[random.Next(serviceTypes.Length)];
                var mode = modes[random.Next(modes.Length)];
                var status = statuses[random.Next(statuses.Length)];
                
                // Higher chance of pending/assigned for future dates
                if (serviceDate > baseDate && random.Next(100) < 70)
                {
                    status = random.Next(100) < 50 ? "Pending" : "Assigned";
                }

                var request = new Request
                {
                    RequestorId = requestor.Id,
                    RequestName = requestor.Name.Contains("Clinic") || requestor.Name.Contains("Hospital") 
                        ? $"Patient Appointment - {requestor.Name}" 
                        : requestor.Name,
                    NumberOfIndividuals = random.Next(1, 3),
                    IndividualType = individualTypes[random.Next(individualTypes.Length)],
                    TypeOfService = serviceType,
                    TypeOfServiceOther = serviceType == "Other" ? "Government meeting" : null,
                    Mode = mode,
                    MeetingLink = mode == "Virtual" ? $"https://zoom.us/j/{random.Next(1000000, 9999999)}" : null,
                    Address = mode == "In-Person" ? requestor.Address?.Split(',')[0] : null,
                    Address2 = mode == "In-Person" && random.Next(100) < 30 ? $"Suite {random.Next(100, 500)}" : null,
                    City = requestor.Address?.Split(',').Length > 1 ? requestor.Address.Split(',')[1].Trim() : "Anchorage",
                    State = "AK",
                    ZipCode = requestor.Address?.Contains("99501") == true ? "99501" 
                        : requestor.Address?.Contains("99508") == true ? "99508"
                        : requestor.Address?.Contains("99701") == true ? "99701"
                        : requestor.Address?.Contains("99801") == true ? "99801"
                        : "99503",
                    GenderPreference = genderPrefs[random.Next(genderPrefs.Length)],
                    PreferredInterpreterId = random.Next(100) < 40 ? interpreters[random.Next(interpreters.Count)].Id : null,
                    PreferredInterpreterName = null,
                    Specializations = random.Next(100) < 60 ? string.Join(", ", specializations.Take(random.Next(1, 3))) : null,
                    ServiceDateTime = serviceDateTime,
                    EndDateTime = endDateTime,
                    Location = mode == "Virtual" ? "Virtual" : requestor.Address,
                    AdditionalNotes = GenerateNotes(serviceType, mode),
                    Status = status
                };

                requests.Add(request);
            }
        }

        return requests;
    }

    public static List<Appointment> GenerateAppointments(List<Request> requests, List<Interpreter> interpreters)
    {
        var random = new Random();
        var appointments = new List<Appointment>();
        var statuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };

        foreach (var request in requests.Where(r => r.Status != "Pending" && r.Status != "Cancelled"))
        {
            // 70% chance of having an appointment
            if (random.Next(100) < 70)
            {
                var interpreter = request.PreferredInterpreterId.HasValue
                    ? interpreters.FirstOrDefault(i => i.Id == request.PreferredInterpreterId.Value)
                    : interpreters[random.Next(interpreters.Count)];

                if (interpreter != null)
                {
                    var status = request.Status == "Completed" ? "Completed" 
                        : request.Status == "Cancelled" ? "Cancelled"
                        : statuses[random.Next(2)]; // Pending or Confirmed

                    var duration = request.EndDateTime.HasValue 
                        ? (int)(request.EndDateTime.Value - request.ServiceDateTime).TotalMinutes
                        : 60;

                    var appointment = new Appointment
                    {
                        RequestId = request.Id,
                        InterpreterId = interpreter.Id,
                        ServiceDateTime = request.ServiceDateTime,
                        Location = request.Location ?? "In-Person",
                        Status = status,
                        ServiceDetails = $"{request.TypeOfService} - {request.Specializations ?? "ASL"}",
                        DurationMinutes = duration,
                        ClientEmployeeName = request.RequestName,
                        AdditionalNotes = request.AdditionalNotes
                    };

                    appointments.Add(appointment);
                }
            }
        }

        return appointments;
    }

    public static List<Invoice> GenerateInvoices(List<Appointment> appointments, List<Requestor> requestors, List<Interpreter> interpreters, List<Request> requests)
    {
        var random = new Random();
        var invoices = new List<Invoice>();
        var paymentStatuses = new[] { "Pending", "Paid", "Overdue" };
        var paymentMethods = new[] { "Check", "Credit Card", "ACH Transfer", null };

        var invoiceNumber = 1;
        foreach (var appointment in appointments.Where(a => a.Status == "Completed" || a.Status == "Confirmed"))
        {
            // 60% chance of having an invoice
            if (random.Next(100) < 60)
            {
                var request = requests.First(r => r.Id == appointment.RequestId);
                var requestor = requestors.First(r => r.Id == request.RequestorId);
                var interpreter = interpreters.First(i => i.Id == appointment.InterpreterId);
                
                var hourlyRate = random.Next(75, 100); // $75-$100 per hour
                var hoursWorked = Math.Round((decimal)appointment.DurationMinutes!.Value / 60, 2);
                var discount = random.Next(100) < 20 ? random.Next(5, 20) : 0; // 20% chance of discount
                var totalCost = (hoursWorked * hourlyRate) - discount;

                var paymentStatus = paymentStatuses[random.Next(paymentStatuses.Length)];
                // Older appointments more likely to be paid
                if (appointment.ServiceDateTime < DateTime.Today.AddDays(-30))
                {
                    paymentStatus = random.Next(100) < 70 ? "Paid" : "Overdue";
                }

                var invoice = new Invoice
                {
                    RequestorId = requestor.Id,
                    AppointmentId = appointment.Id,
                    InterpreterId = interpreter.Id,
                    ServiceType = request.TypeOfService ?? "General",
                    HoursWorked = hoursWorked,
                    HourlyRate = hourlyRate,
                    Discount = discount,
                    TotalCost = totalCost,
                    PaymentStatus = paymentStatus,
                    InvoiceNumber = $"INV-2026-{invoiceNumber:D4}",
                    PaymentMethod = paymentStatus == "Paid" ? paymentMethods[random.Next(paymentMethods.Length - 1)] : null,
                    Notes = $"Invoice for {appointment.ServiceDetails} on {appointment.ServiceDateTime:MM/dd/yyyy}"
                };

                invoices.Add(invoice);
                invoiceNumber++;
            }
        }

        return invoices;
    }

    private static string GenerateNotes(string serviceType, string mode)
    {
        var notes = new Dictionary<string, string[]>
        {
            ["Medical"] = new[] 
            { 
                "Patient is Deaf. Medical terminology may be complex.",
                "Annual check-up appointment.",
                "Follow-up visit. Patient prefers same interpreter.",
                "Surgery consultation. Requires experienced medical interpreter.",
                "Emergency department visit."
            },
            ["Legal"] = new[] 
            { 
                "Court appearance. Certified interpreter required.",
                "Client meeting. Confidential matter.",
                "Deposition scheduled.",
                "Legal consultation.",
                "Hearing scheduled. Video call."
            },
            ["Educational"] = new[] 
            { 
                "Classroom interpreting for semester.",
                "Parent-teacher conference.",
                "School event interpreting.",
                "IEP meeting.",
                "Graduation ceremony."
            },
            ["Other"] = new[] 
            { 
                "Government meeting.",
                "Community event.",
                "Workshop attendance.",
                "Conference interpreting.",
                "General consultation."
            }
        };

        var serviceNotes = notes.ContainsKey(serviceType) ? notes[serviceType] : notes["Other"];
        var random = new Random();
        var baseNote = serviceNotes[random.Next(serviceNotes.Length)];
        
        if (mode == "Virtual")
        {
            baseNote += " Virtual appointment. Meeting link will be provided.";
        }

        return baseNote;
    }
}
