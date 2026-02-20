# Request and Appointment Workflow

## Overview
This document defines the proper workflow for handling interpreter service requests and appointments from initial submission through completion and payment.

## Two-Tier Status System

### Requests
Requests represent the initial application or ask for interpreter services. They have four possible statuses:
- **New Request**: Public form submissions (from /Request page)
- **Reviewed**: Staff has reviewed and verified details
- **Approved**: Ready for interpreter assignment (admin-created requests start here)
- **Broadcasted**: Request has been sent out to selected interpreters.  

### Appointments
Appointments are created when an interpreter is assigned to an approved request. They track the actual service delivery:
- **Assigned**: Interpreter assigned to request
- **Confirmed**: Both parties confirmed
- **Completed**: Service rendered
- **Cancelled<48h**: Canceled with less than 48 hours notice (charge applies)
- **Cancelled>48h**: Canceled with more than 48 hours notice (no charge)
- **Paid**: Invoice paid

## Request Status Definitions

### 1. **New Request** (Yellow - #FFC107)
- **When**: Automatically set when a request is submitted
- **Description**: Initial status for all incoming requests
- **Action Required**: Staff should review the request details to ensure completeness and validity
- **Next Step**: Move to "Reviewed" after initial review

### 2. **Reviewed** (Yellow - #FFC107)
- **When**: Staff has reviewed the request and verified all details
- **Description**: Request has been checked for completeness, accuracy, and feasibility
- **Action Required**: Approve or reject the request
- **Next Step**: Move to "Approved" if acceptable, or contact requestor for clarification

### 3. **Approved** (Yellow - #FFC107)
- **When**: Request is approved and ready for interpreter assignment
  - **Admin-created requests**: Automatically set to "Approved"
  - **Public requests**: Set after review and approval
- **Description**: Request is valid and ready to have an interpreter assigned
- **Action Required**: Assign an interpreter by creating an appointment
- **Next Step**: Create an appointment to assign interpreter (appointment will have "Assigned" status)

### 4. **Broadcasted** (Blue - #0D6EFD)
- **When**: Request has been sent to selected interpreters for interest/availability
- **Description**: Interpreters have been notified of the available request
- **Action Required**: Wait for interpreter responses, then create appointment with accepting interpreter
- **Next Step**: Create an appointment with the interpreter who accepts the request

## Appointment Status Definitions

### 1. **Assigned** (Aqua - #00CED1)
- **When**: An interpreter has been assigned to an approved request (appointment created)
- **Description**: Appointment has a designated interpreter
- **Action Required**: Confirm with interpreter and notify requestor
- **Next Step**: Move to "Confirmed" after both parties confirm

### 2. **Confirmed** (Aqua - #00CED1)
- **When**: Both interpreter and requestor have confirmed the appointment
- **Description**: All parties are confirmed and ready for service
- **Action Required**: Service should be provided on scheduled date
- **Next Step**: Move to "Completed" after service is rendered

### 3. **Completed** (Orange - #FFA500)
- **When**: Service has been completed successfully
- **Description**: Interpreter provided the requested service
- **Action Required**: Create invoice for payment
- **Next Step**: Move to "Paid" after invoice is paid

### 4. **Cancelled<48h** (Orange - #FFA500)
- **When**: Appointment is cancelled with less than 48 hours notice
- **Description**: Cancellation with charge applies (standard policy)
- **Action Required**: Create invoice for cancellation fee
- **Next Step**: Move to "Paid" after fee is paid
- **Policy**: Client is charged for late cancellation

### 5. **Cancelled>48h** (Red - #DC3545)
- **When**: Appointment is cancelled with more than 48 hours notice
- **Description**: Cancellation with no charge (standard policy)
- **Action Required**: Close appointment, no invoice needed
- **Next Step**: N/A - Appointment is closed
- **Policy**: No charge to client

### 6. **Paid** (Green - #28A745)
- **When**: Invoice has been paid
- **Description**: Appointment is financially complete
- **Action Required**: Archive/close appointment
- **Next Step**: N/A - Appointment is complete

## Recommended Workflow

### Request Flow (Public Submissions)
```
New Request → Reviewed → Approved → Create Appointment
              ↓           ↓
         Contact for  Reject/Return
         Clarification
```

### Request Flow (Admin Created)
```
Approved → Create Appointment
(Admin-created requests skip New Request and Reviewed stages)
```

### Appointment Flow
```
Assigned → Confirmed → Completed → Paid
    ↓
  Cancel (choose <48h or >48h based on notice given)
```

## Important Notes

### Request Statuses
- **Public requests** (from /Request page): Default to "New Request"
- **Admin-created requests** (from /Requests/Create): Default to "Approved"
- Requests only track the approval process, not service delivery
- Once approved, create an appointment to assign an interpreter

### Before Creating Appointments
- **Requests must be in "Approved" status before creating appointments**
- Ensure all request details are complete:
  - Service date/time
  - Location or virtual meeting link
  - Type of service
  - Special requirements or interpreter specializations
  - Consumer names
  - Contact information

### Appointment Management
- Appointments track the actual service delivery and billing
- Create appointments from approved requests via the Appointments/Create page
- Appointment status changes are independent of request status

### Cancellation Policy (Appointments)
- **Less than 48 hours**: Client is charged cancellation fee (Cancelled<48h)
- **More than 48 hours**: No charge to client (Cancelled>48h)
- Always select the appropriate cancellation status to ensure proper billing
- Cancellation statuses only apply to appointments, not requests

### Status Changes
- **Request status** can be changed manually in the Request Edit page
  - Available statuses: New Request, Reviewed, Approved, Broadcasted
  - Status automatically changes to Broadcasted when using "Notify Interpreters" feature
- **Appointment status** can be changed manually in the Appointment Edit page
  - Available statuses: Assigned, Confirmed, Completed, Cancelled<48h, Cancelled>48h, Paid
- Creating an appointment does not automatically update request status
- Request and appointment statuses are tracked separately

## Color Coding Summary

### Request Statuses
| Status | Color | Hex Code | Meaning |
|--------|-------|----------|---------|
| New Request | Yellow | #FFC107 | Needs initial review |
| Reviewed | Yellow | #FFC107 | Reviewed, needs approval |
| Approved | Yellow | #FFC107 | Ready for interpreter assignment || Broadcasted | Blue | #0D6EFD | Sent to interpreters for interest |
### Appointment Statuses
| Status | Color | Hex Code | Meaning |
|--------|-------|----------|---------|
| Assigned | Aqua | #00CED1 | Interpreter assigned |
| Confirmed | Aqua | #00CED1 | Confirmed by all parties |
| Completed | Orange | #FFA500 | Service completed |
| Cancelled<48h | Orange | #FFA500 | Late cancel, charge applies |
| Cancelled>48h | Red | #DC3545 | Early cancel, no charge |
| Paid | Green | #28A745 | Financially complete |

## Calendar Display
- **Requests** display with their status (New Request, Reviewed, Approved, Broadcasted) with color coding
- **Appointments** display with their status color coding for visual workflow tracking
- This two-tier system allows clear separation between request approval and service delivery
