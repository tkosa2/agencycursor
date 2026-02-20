# AgencyCursor - Interpreter Agency Management System

**Tech Stack**: .NET 8 Razor Pages, SQLite, Entity Framework Core, Bootstrap 5, Playwright E2E Testing

---

## Table of Contents
1. [Overview](#overview)
2. [Application Requirements](#application-requirements)
3. [Core Entities](#core-entities)
4. [Request Workflow](#request-workflow)
5. [Appointment Workflow](#appointment-workflow)
6. [Form Elements](#form-elements)
7. [Key Features](#key-features)
8. [Status Reference](#status-reference)
9. [Getting Started](#getting-started)

---

## Overview

AgencyCursor is a comprehensive web-based interpreter agency management system designed to streamline the process of:
- **Receiving interpreter service requests** from the public and internal staff
- **Managing interpreter availability** and specializations
- **Broadcasting requests** to qualified interpreters
- **Tracking interpreter responses** with a hybrid email + web UI system
- **Scheduling appointments** and assigning interpreters
- **Managing billing** through appointment completion and invoicing

The system handles both Deaf/Deaf-Blind service requests and enforces a professional two-tier status model for Request and Appointment tracking.

---

## Application Requirements

### Functional Requirements

#### 1. Request Management
- ✅ Public request submission form for requestors
- ✅ Admin request creation for internal use
- ✅ Request status tracking (New Request → Reviewed → Approved → Broadcasted)
- ✅ Request filtering by status
- ✅ Email broadcast to multiple interpreters with personalized response links
- ✅ Interpreter response tracking (Yes/No/Maybe with optional notes)

#### 2. Appointment Management
- ✅ Create appointments from approved requests
- ✅ Appointment status tracking (Assigned → Confirmed → Completed → Paid)
- ✅ Cancellation policies (48-hour notice distinction)
- ✅ Appointment assignment to interpreters
- ✅ Automatic appointment creation when broadcasting to interpreters

#### 3. Interpreter Management
- ✅ Interpreter listing and search
- ✅ Interpreter language/skill specializations
- ✅ Interpreter availability calendar
- ✅ Contact information management
- ✅ Response dashboard tracking

#### 4. Email Communication
- ✅ SMTP-based email broadcasts to interpreters
- ✅ Personalized response URLs unique per interpreter
- ✅ Development mode email redirection (prevents test emails to real addresses)
- ✅ HTML email templates with request details and CTA buttons

#### 5. Response Tracking
- ✅ Interpreter response system (Yes/No/Maybe status with notes)
- ✅ Response persistence and update capability
- ✅ Admin dashboard showing all responses per request
- ✅ Color-coded response indicators (Green=Yes, Yellow=Maybe, Red=No)

#### 6. Billing & Invoicing
- ✅ Invoice creation from appointments
- ✅ Automatic cost calculation
- ✅ Cancellation fee handling based on notice period

### Non-Functional Requirements
- ✅ Responsive design (mobile, tablet, desktop)
- ✅ Fast performance with SQLite backend
- ✅ Comprehensive test coverage (E2E and unit tests)
- ✅ Secure email communication
- ✅ Data persistence and migration support

---

## Core Entities

### 1. **Requestors**
Client organizations or individuals requesting interpreter services.

**Fields:**
- Requestor First Name
- Requestor Last Name
- Phone Number
- Email Address
- Notes (optional)

---

### 2. **Interpreters**
Professional interpreters available to provide services.

**Fields:**
- First Name
- Last Name
- Email
- Phone
- Language/Specialization (ASL, Spanish, Russian, etc.)
- Certification Status
- Notes
- Availability

---

### 3. **Requests**
Service requests submitted by requestors (public) or created by staff (admin).

**Fields:**
- Request ID
- Requestor (Link to Requestor entity)
- Request Name (optional description)
- Number of Deaf/Deaf-Blind Individuals
- Individual Type (Deaf or Deaf-Blind)
- Type of Service (Medical, Legal, Educational, Other)
- Mode (In-Person or Virtual)
- Appointment Date & Time
- Address (for In-Person)
- Virtual Meeting Link (for Virtual appointments)
- Gender Preference (optional)
- Preferred Interpreter (optional)
- Special Requirements or Notes
- Status (New Request, Reviewed, Approved, Broadcasted)

---

### 4. **Appointments**
Scheduled service delivery instances linked to approved requests.

**Fields:**
- Request ID (Link to Request)
- Interpreter ID (Link to Interpreter)
- Appointment Date & Time
- Duration
- Location Details
- Status (Assigned, Confirmed, Completed, Cancelled<48h, Cancelled>48h, Paid)
- Notes

---

### 5. **Interpreter Responses**
Tracks interpreter responses to broadcast requests.

**Fields:**
- Request ID
- Interpreter ID
- Response Status (Yes, No, Maybe)
- Notes (optional)
- Responded At (timestamp)
- Created At (timestamp)

---

### 6. **Invoices**
Billing records for completed or cancelled appointments.

**Fields:**
- Invoice ID
- Appointment ID
- Amount
- Status (Pending, Paid)
- Created Date
- Paid Date

---

## Request Workflow

### Overview
Requests go through a two-tier approval system before interpreters are assigned:

```
Public Submission: New Request → Reviewed → Approved → Broadcasted
Admin Creation:                            ↓
                                    (Appointment Created)
```

### Detailed Request Status Flow

#### 1. **New Request** (Yellow - #FFC107)
- **When**: Automatically set when a request is submitted via public form
- **Description**: Initial status for all incoming requests
- **Action Required**: Staff should review the request details
- **Next Step**: Move to "Reviewed"

#### 2. **Reviewed** (Yellow - #FFC107)
- **When**: Staff has reviewed and verified all details
- **Description**: Request checked for completeness and feasibility
- **Action Required**: Approve or request clarification from requestor
- **Next Step**: Move to "Approved" if acceptable

#### 3. **Approved** (Yellow - #FFC107)
- **When**: Request is validated and ready for interpreter assignment
  - **Public requests**: Manually set after review
  - **Admin-created requests**: Automatically set upon creation
- **Description**: Request is valid and ready for broadcasting/assignment
- **Action Required**: Either assign interpreter directly or broadcast to multiple interpreters
- **Next Step**: Create appointment or move to "Broadcasted"

#### 4. **Broadcasted** (Blue - #0D6EFD)
- **When**: Request has been sent to selected interpreters via email
- **Description**: Interpreters have been notified with personalized response links
- **Action Required**: Wait for interpreter responses and review engagement
- **Next Step**: Create appointment with accepting interpreter

---

## Appointment Workflow

### Overview
Appointments manage the actual service delivery and billing:

```
Assigned → Confirmed → Completed → Paid
    ↓
  Cancel (with <48h or >48h determination)
```

### Detailed Appointment Status Flow

#### 1. **Assigned** (Aqua - #00CED1)
- **When**: Interpreter assigned to approved request (appointment created)
- **Description**: Appointment has designated interpreter
- **Action Required**: Confirm with both parties
- **Next Step**: Move to "Confirmed"

#### 2. **Confirmed** (Aqua - #00CED1)
- **When**: Both interpreter and requestor have confirmed
- **Description**: All parties ready for service
- **Action Required**: Service to be provided on scheduled date
- **Next Step**: Move to "Completed" after service rendered

#### 3. **Completed** (Orange - #FFA500)
- **When**: Service has been successfully completed
- **Description**: Interpreter provided the requested service
- **Action Required**: Create invoice for payment
- **Next Step**: Move to "Paid" after invoice is paid

#### 4. **Cancelled<48h** (Orange - #FFA500)
- **When**: Cancelled with less than 48 hours notice
- **Description**: Cancellation fee applies per policy
- **Action Required**: Create invoice for cancellation fee
- **Policy**: Client is charged standard cancellation fee
- **Next Step**: Move to "Paid" after fee payment

#### 5. **Cancelled>48h** (Red - #DC3545)
- **When**: Cancelled with 48+ hours notice
- **Description**: Cancellation with no charge
- **Action Required**: Close appointment (no invoice needed)
- **Policy**: No charge to client
- **Next Step**: N/A - Appointment closed

#### 6. **Paid** (Green - #28A745)
- **When**: Invoice has been paid
- **Description**: Appointment is financially complete
- **Action Required**: Archive/close appointment
- **Next Step**: N/A - Complete

---

## Form Elements

### Public Request Form (`/Request`)

#### Section 1: Requestor Information
- **Requestor First Name** (required) - Text field
- **Requestor Last Name** (required) - Text field
- **Request Name** (optional) - Text field
- **Number of Deaf/Deaf-Blind Individuals** (required) - Number (1-99)
- **Individual Type** (required) - Radio buttons: Deaf, Deaf-Blind

#### Section 2: Contact Information
- **Phone Number** (required) - Tel field
- **Email Address** (required) - Email field

#### Section 3: Appointment Details
- **Date of Appointment** (required) - Date picker
- **Start Time** (required) - Time field
- **End Time** (auto-calculated) - Read-only (2-hour minimum default)

#### Section 4: Type of Appointment
- **Type of Service** (required) - Radio buttons:
  - Medical
  - Legal
  - Educational
  - Other (with text field for specification)

#### Section 5: Mode of Interpretation
- **Mode** (required) - Radio buttons: In-Person, Virtual

#### Section 6: Virtual Appointment Information (Conditional)
- Shows when Virtual mode is selected
- **Meeting Information** (required if Virtual) - TextArea with placeholder for link, passcode, phone, meeting ID

#### Section 7: In-Person Appointment Location (Conditional)
- Shows when In-Person mode is selected
- **Address** (required) - Text field (street address)
- **Address Line 2** (optional) - Text field (suite, floor, apartment)
- **City** (required) - Text field
- **State** (required) - Dropdown select with all US states
- **ZIP Code** (required) - Pattern validation for 5-digit or ZIP+4

#### Section 8: Interpreter Preferences
- **Gender Preference** (optional) - Radio buttons: Male, Female, No Preference
- **Preferred Interpreter** (optional) - Text field with autocomplete

#### Section 9: Special Requirements
- **Special Terminology or Accessibility Needs** (optional) - TextArea
- **Additional Information** (optional) - TextArea

#### Section 10: Agreement & Confirmation
- **Terms Agreement** (required) - Checkbox with agreement text

---

### Admin Request Form (`/Requests/Create`)

Similar structure to public form but:
- Pre-sets status to "Approved"
- Includes all requestor fields editable
- Can directly assign interpreter during creation
- Requestor search/autocomplete from existing requestors

---

### Interpreter Response Form (`/Interpreters/RespondToRequest/{requestId}/{interpreterId}`)

#### Display Information
- Full request details (read-only):
  - Service type, date/time, location
  - Special requirements
  - Number of individuals

#### Response Section
- **Response Status** (required) - Radio buttons:
  - Yes (green)
  - No (red)
  - Maybe (yellow)
- **Optional Notes** - TextArea with placeholder guidance
- **Submit Button** - "Submit Response"

#### Previous Response Display (if exists)
- Shows previous response with timestamp
- Allows updating response by selecting new option and resubmitting

---

### Interpreter Management Form (`/Interpreters/Create`)

- **First Name** (required)
- **Last Name** (required)
- **Email** (required)
- **Phone** (required)
- **Language/Specialization** (required) - Checkboxes or multi-select
- **Certification** (optional)
- **Availability** (optional)
- **Notes** (optional)

---

### Appointment Form (`/Appointments/Create`)

- **Request** (required) - Dropdown (filtered to "Approved" status only)
- **Interpreter** (required) - Dropdown with search
- **Appointment Date** (required) - Date picker
- **Start Time** (required) - Time field
- **End Time** (auto-calculated) - Read-only
- **Location Details** (required) - TextArea
- **Notes** (optional) - TextArea

---

### Invoice Form (`/Invoices/Create`)

- **Appointment** (required) - Dropdown select
- **Amount** (auto-calculated from appointment duration)
- **Cancellation Fee** (conditional) - If Cancelled<48h appointment
- **Notes** (optional) - TextArea

---

## Key Features

### 1. Email Broadcast System
- ✅ Sends personalized emails to each interpreter with unique response link
- ✅ Includes full request details in email body
- ✅ Development mode redirects all emails to test address (tkosa3@gmail.com)
- ✅ Production mode sends to actual interpreter emails
- ✅ Email template includes CTA button with response link

### 2. Interpreter Response Dashboard
- ✅ Color-coded response status badges
- ✅ Shows interpreter name, response, notes, timestamp
- ✅ Admin can see all responses at a glance
- ✅ Interpreters can update responses by clicking email link again
- ✅ Response URL format: `/Interpreters/RespondToRequest/{requestId}/{interpreterId}`

### 3. Request Status Filtering
- ✅ Filter requests list by status
- ✅ Quick access to New Requests, Reviewed, Approved, Broadcasted statuses
- ✅ Visual status badges with consistent color coding

### 4. Two-Tier Status System
- ✅ Requests track approval workflow (New → Reviewed → Approved → Broadcasted)
- ✅ Appointments track service delivery (Assigned → Confirmed → Completed → Paid)
- ✅ Clear separation of concerns between request approval and service delivery

### 5. Responsive Design
- ✅ Mobile-optimized request form with conditional sections
- ✅ Bootstrap 5 grid system for all layouts
- ✅ Auto-calculated fields (End Time from Start Time)
- ✅ Accessible form labels and validation messages

### 6. SMTP Email Service
- ✅ Mailtrap integration for email testing
- ✅ HTML email templates
- ✅ Batch email sending capability
- ✅ Development-only email redirection for safety

### 7. Appointment Duration Calculation
- ✅ Default 2-hour minimum for interpreter requests
- ✅ Auto-calculates end time from start time
- ✅ Read-only end time field to prevent manual errors

### 8. Request Search & Autocomplete
- ✅ Search existing requestors by name, email, or phone
- ✅ Auto-populate requestor fields from search results
- ✅ Quick access to repeat requestors

---

## Status Reference

### Request Status Colors & Meanings

| Status | Color | Hex | Meaning |
|--------|-------|-----|---------|
| New Request | Yellow | #FFC107 | Initial submission, awaiting review |
| Reviewed | Yellow | #FFC107 | Staff has reviewed, needs approval decision |
| Approved | Yellow | #FFC107 | Ready for interpreter assignment |
| Broadcasted | Blue | #0D6EFD | Sent to interpreters, waiting for responses |

### Appointment Status Colors & Meanings

| Status | Color | Hex | Meaning |
|--------|-------|-----|---------|
| Assigned | Aqua | #00CED1 | Interpreter assigned, confirmation pending |
| Confirmed | Aqua | #00CED1 | All parties confirmed, ready for service |
| Completed | Orange | #FFA500 | Service rendered, awaiting payment |
| Cancelled<48h | Orange | #FFA500 | Late cancellation, fee applies |
| Cancelled>48h | Red | #DC3545 | Early cancellation, no charge |
| Paid | Green | #28A745 | Financially complete |

### Interpreter Response Status Colors & Meanings

| Status | Color | Meaning |
|--------|-------|---------|
| Yes | Green | Interpreter accepts the request |
| Maybe | Yellow | Interpreter interested, pending clarification |
| No | Red | Interpreter cannot fulfill request |

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQLite
- Visual Studio or VS Code with C# extension

### Installation & Running

1. **Clone the repository**
   ```bash
   git clone https://github.com/tkosa2/agencycursor.git
   cd agencycursor
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run database migrations**
   ```bash
   cd AgencyCursor.WebApp
   dotnet ef database update --context AgencyDbContext
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Navigate to `https://localhost:5001` (or `http://localhost:5000`)
   - Public request form: `/Request`
   - Admin panel: `/Requests`, `/Interpreters`, `/Appointments`

### Running Tests

```bash
cd ../AgencyCursor.Tests
dotnet test
```

All E2E tests use Playwright browser automation with xUnit framework.

---

## Development Configuration

### Email Service (appsettings.Development.json)
```json
{
  "SmtpSettings": {
    "Host": "smtp.mailtrap.io",
    "Port": 587,
    "Username": "your-mailtrap-user",
    "Password": "your-mailtrap-password",
    "TestEmailAddress": "tkosa3@gmail.com"
  }
}
```

**Development Mode Behavior**: All emails redirect to `TestEmailAddress` to prevent accidental test emails reaching real interpreters.

---

## Database

### Tables
- Requestors
- Interpreters
- Requests
- Appointments
- InterpreterResponses
- Invoices
- ZipCodes (reference data for form validation)

### Migrations
Handled via Entity Framework Core with code-first approach. All migrations are version-controlled and applied automatically on startup.
