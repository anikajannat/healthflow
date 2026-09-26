# HealthFlow — Complete Angular + ASP.NET Core + SQL Server Healthcare Project

This package contains the updated HealthFlow project with a polished doctor discovery and appointment-booking experience, service pages, OTP email support, SQL Server, and a bKash-style payment flow for development/testing.

## Main stack

- Frontend: Angular 20 standalone components
- Backend: ASP.NET Core Web API on .NET 9
- Database: Microsoft SQL Server + Entity Framework Core
- Authentication: JWT + email OTP
- Email: SMTP / Gmail App Password supported
- Payment: bKash-style Mock/Sandbox flow (live merchant credentials are not included)
- Deployment: Angular static site + Dockerized .NET API + external SQL Server/Azure SQL

## New doctor-booking experience

The `/doctors` page now provides:

- Doctor-name dropdown
- Specialty dropdown
- Free-text doctor/specialty/qualification search
- Doctor cards with bio, qualifications, experience, rating and consultation fee
- Doctor profile page
- `Book an Appointment` CTA
- Upcoming published schedules for each doctor
- Multiple future dates, not only today's schedule
- 20-minute time slots
- Past/booked slots disabled
- Three-step booking flow: Schedule → Visit Details → Payment
- bKash-style payment card and mobile-number validation
- Confirmation screen with serial number and transaction ID

## Important bKash note

The included payment flow uses `Payment:Mode = Mock`. It gives you a complete UI/backend booking demonstration and stores the payment as `bKash Mock` with a generated transaction ID.

A real bKash Checkout integration needs an approved bKash merchant account and official credentials/API contract. Do not put real secrets in source control.

## Local run — the setup matching your current Windows machine

### 1. SQL Server

The default connection is Windows Authentication:

```text
Server=localhost;Database=HealthcareDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
```

Open SQL Server Management Studio and make sure you can connect to `localhost` using Windows Authentication.

### 2. Backend in Visual Studio 2022

Open:

```text
backend/Healthcare.sln
```

This version targets `.NET 9`, so it is compatible with your Visual Studio 2022 17.14 setup as long as the .NET 9 SDK / ASP.NET workload is installed.

Run `Healthcare.Api` using the `http` profile. Default URL:

```text
http://localhost:5080
```

Health test:

```text
http://localhost:5080/health
```

### 3. Frontend

From PowerShell:

```powershell
cd frontend
npm install
npm start
```

Open:

```text
http://localhost:4200
```

## Demo doctors and schedules

`appsettings.Development.json` enables:

```json
{
  "Seed": {
    "DemoData": true
  }
}
```

In Development, if there are no doctors, the backend creates three approved demo doctors. It also adds published future schedules for approved doctors that do not already have schedules on those dates. This lets the booking UI show real schedule cards immediately.

Set `Seed:DemoData` to `false` when you do not want automatic demo scheduling.

Demo doctor password (only when the demo doctors are newly seeded):

```text
Doctor123!
```

## Super Admin

Default local seed:

```text
Email: superadmin@healthflow.local
Password: ChangeMe123!
```

Change this before production.

## Gmail OTP

To send OTPs to real email addresses, configure `Smtp` in `appsettings.json` or use environment variables.

Example:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "User": "YOUR_SENDER_GMAIL@gmail.com",
  "Password": "YOUR_16_CHARACTER_GOOGLE_APP_PASSWORD",
  "From": "YOUR_SENDER_GMAIL@gmail.com",
  "EnableSsl": true
}
```

Use a Google App Password, not your normal Gmail password. Keep the real App Password private.

If SMTP is blank, the API logs the OTP to the backend console for development.

## Booking rules

A doctor must be:

- Approved
- Active
- Have a Published schedule

Patients can book a future slot from today or upcoming dates. The backend rejects past slots and already-booked slots.

## Key URLs

```text
Home:                 http://localhost:4200
Doctors:              http://localhost:4200/doctors
Doctor profile:       /doctors/{doctorId}
Appointment booking:  /book/{doctorId}
Patient dashboard:    /patient
Doctor dashboard:     /doctor
Admin dashboard:      /admin
API:                  http://localhost:5080
API health:           http://localhost:5080/health
```

## Project layout

```text
HealthcareSystem-Angular-DotNet-Complete/
├─ backend/
│  ├─ Healthcare.sln
│  └─ Healthcare.Api/
│     ├─ Controllers/
│     ├─ Data/
│     ├─ DTOs/
│     ├─ Models/
│     ├─ Services/
│     ├─ Program.cs
│     └─ appsettings.json
├─ frontend/
│  └─ src/app/
│     ├─ core/
│     └─ pages/
│        ├─ doctors/
│        ├─ doctor-detail/
│        ├─ book-appointment/
│        └─ service-detail/
└─ render.yaml
```

## Production checklist

Before real deployment:

- Use EF Core migrations instead of only `EnsureCreatedAsync()`
- Disable `Seed:DemoData`
- Change Super Admin credentials
- Use environment variables for JWT, database and SMTP secrets
- Replace Mock bKash with approved live merchant integration
- Add audit logging and rate limiting
- Review privacy/security requirements before storing real patient data
