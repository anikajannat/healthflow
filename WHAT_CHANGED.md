# What changed in this build

- Upgraded backend target to .NET 9 for Visual Studio 2022 17.14 compatibility.
- Fixed patient registration flow so welcome-email failure does not destroy a successful account creation.
- Added JSON cycle handling and safer user DTO output.
- Added robust Gmail SMTP/App Password handling.
- Added `Our Services` dropdown and individual service detail pages.
- Redesigned doctor directory around doctor-name + specialty selection and profile cards.
- Added doctor profile pages.
- Added public per-doctor upcoming schedule API for the next 30 days.
- Booking now supports future dates and blocks only past/booked 20-minute slots.
- Added a three-step appointment flow: schedule, visit details, payment.
- Added bKash-style Mock/Sandbox checkout with transaction ID, payment record and invoice.
- Added demo doctors/schedules in Development mode so the UI is immediately testable.
- Updated patient dashboard to show payment provider and transaction ID.
- Updated home page with services, consultation CTA, booking journey and payment messaging.
