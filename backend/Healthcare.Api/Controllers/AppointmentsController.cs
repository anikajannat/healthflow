using Healthcare.Api.Data;
using Healthcare.Api.DTOs;
using Healthcare.Api.Helpers;
using Healthcare.Api.Models;
using Healthcare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Authorize]
public class AppointmentsController(AppDbContext db, IEmailService email, SimplePdfService pdf, BkashPaymentService bkash) : ControllerBase
{
    [Authorize(Roles = "Patient")]
    [HttpPost("book")]
    public async Task<IActionResult> Book(BookAppointmentRequest request)
    {
        var schedule = await db.DoctorSchedules
            .Include(x => x.DoctorProfile).ThenInclude(x => x.User)
            .Include(x => x.Appointments)
            .FirstOrDefaultAsync(x => x.Id == request.ScheduleId);

        if (schedule is null || schedule.Status != ScheduleStatus.Published)
            return BadRequest(new { message = "Schedule is not bookable." });
        if (schedule.DoctorProfile.ApprovalStatus != DoctorApprovalStatus.Approved || schedule.DoctorProfile.User.Status != UserStatus.Active)
            return BadRequest(new { message = "Doctor is not currently available for booking." });

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (schedule.Date < today) return BadRequest(new { message = "Past schedules cannot be booked." });

        var totalSlots = (int)((schedule.EndTime.ToTimeSpan() - schedule.StartTime.ToTimeSpan()).TotalMinutes / 20);
        if (request.SlotIndex < 0 || request.SlotIndex >= totalSlots)
            return BadRequest(new { message = "Invalid slot." });

        var localStart = schedule.Date.ToDateTime(schedule.StartTime.AddMinutes(request.SlotIndex * 20));
        if (localStart <= DateTime.Now)
            return BadRequest(new { message = "Please choose a future time slot." });

        if (schedule.Appointments.Any(x => x.SlotIndex == request.SlotIndex && x.Status != AppointmentStatus.Cancelled))
            return Conflict(new { message = "That slot was just booked. Please choose another." });

        var paymentMethod = (request.PaymentMethod ?? "Bkash").Trim();
        if (!paymentMethod.Equals("Bkash", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "This demo currently supports bKash checkout only." });

        var paymentResult = await bkash.PayAsync(request.BkashNumber, schedule.DoctorProfile.ConsultationFee, Guid.NewGuid());
        if (!paymentResult.Success) return BadRequest(new { message = paymentResult.Message });

        var serial = schedule.Appointments.Count(x => x.Status != AppointmentStatus.Cancelled) + 1;
        var appointment = new Appointment
        {
            ScheduleId = schedule.Id,
            PatientUserId = User.UserId(),
            SlotIndex = request.SlotIndex,
            SerialNumber = serial,
            AppointmentStartUtc = localStart.ToUniversalTime(),
            Reason = request.Reason,
            Status = AppointmentStatus.Booked,
            Payment = new Payment
            {
                Amount = schedule.DoctorProfile.ConsultationFee,
                Status = PaymentStatus.Paid,
                Provider = paymentResult.Mode.Equals("Mock", StringComparison.OrdinalIgnoreCase) ? "bKash Mock" : "bKash",
                TransactionId = paymentResult.TransactionId,
                PaidAt = DateTime.UtcNow
            }
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        var patient = await db.Users.FirstAsync(x => x.Id == User.UserId());
        var invoice = pdf.Create("HealthFlow Appointment Invoice", new[]
        {
            $"Patient: {patient.Name}",
            $"Doctor: {schedule.DoctorProfile.User.Name}",
            $"Specialty: {schedule.DoctorProfile.Specialization}",
            $"Date: {schedule.Date:yyyy-MM-dd}",
            $"Slot: {localStart:hh:mm tt}",
            $"Serial: {serial}",
            $"Paid via: {appointment.Payment.Provider}",
            $"Transaction: {appointment.Payment.TransactionId}",
            $"Paid: BDT {schedule.DoctorProfile.ConsultationFee:0.00}",
            $"Meet link: {schedule.MeetLink}"
        });

        try
        {
            await email.SendAsync(patient.Email, "Appointment confirmed",
                "Your HealthFlow appointment is confirmed. Your invoice is attached.", invoice, "invoice.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Appointment email failed: {ex.Message}");
        }

        return Ok(new
        {
            message = "Appointment booked successfully.",
            appointment.Id,
            appointment.SerialNumber,
            appointmentStart = localStart,
            payment = new
            {
                appointment.Payment.Amount,
                provider = appointment.Payment.Provider,
                appointment.Payment.TransactionId,
                status = appointment.Payment.Status.ToString()
            }
        });
    }

    [Authorize(Roles = "Patient")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var uid = User.UserId();
        var items = await db.Appointments.AsNoTracking().Include(x => x.Schedule).ThenInclude(x => x.DoctorProfile).ThenInclude(x => x.User)
            .Include(x => x.Payment).Include(x => x.Prescription)
            .Where(x => x.PatientUserId == uid).OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id, status = x.Status.ToString(), x.SerialNumber, x.SlotIndex, x.Reason, x.AppointmentStartUtc,
                doctor = x.Schedule.DoctorProfile.User.Name, x.Schedule.DoctorProfile.Specialization,
                x.Schedule.MeetLink, scheduleDate = x.Schedule.Date, x.Schedule.StartTime,
                payment = x.Payment == null ? null : new { x.Payment.Amount, status = x.Payment.Status.ToString(), x.Payment.Provider, x.Payment.TransactionId },
                prescription = x.Prescription == null ? null : new { x.Prescription.Id, x.Prescription.Findings, x.Prescription.Medicines, x.Prescription.Advice }
            }).ToListAsync();
        return Ok(items);
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor")]
    public async Task<IActionResult> DoctorAppointments()
    {
        var uid = User.UserId();
        var items = await db.Appointments.AsNoTracking().Include(x => x.PatientUser)
            .Include(x => x.Schedule).ThenInclude(x => x.DoctorProfile)
            .Where(x => x.Schedule.DoctorProfile.UserId == uid)
            .OrderByDescending(x => x.AppointmentStartUtc)
            .Select(x => new { x.Id, patient = x.PatientUser.Name, x.PatientUser.Email, x.SerialNumber, x.Reason, x.AppointmentStartUtc, status = x.Status.ToString() })
            .ToListAsync();
        return Ok(items);
    }

    [Authorize(Roles = "Patient")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var appointment = await db.Appointments.Include(x => x.Schedule).Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id == id && x.PatientUserId == User.UserId());
        if (appointment is null) return NotFound();
        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            return BadRequest(new { message = "Appointment cannot be cancelled." });

        var scheduleStartLocal = appointment.Schedule.Date.ToDateTime(appointment.Schedule.StartTime);
        var refund = DateTime.Now < scheduleStartLocal.AddHours(-1);

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        if (refund && appointment.Payment?.Status == PaymentStatus.Paid)
        {
            appointment.Payment.Status = PaymentStatus.Refunded;
            appointment.Payment.RefundedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        return Ok(new { message = refund ? "Cancelled with refund." : "Cancelled without refund.", refund });
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost("{id:guid}/status/{status}")]
    public async Task<IActionResult> SetStatus(Guid id, string status)
    {
        var appointment = await db.Appointments.Include(x => x.Schedule).ThenInclude(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.Schedule.DoctorProfile.UserId == User.UserId());
        if (appointment is null) return NotFound();
        if (!Enum.TryParse<AppointmentStatus>(status, true, out var next) || next is AppointmentStatus.Cancelled)
            return BadRequest(new { message = "Doctor can set Ongoing or Completed." });

        var valid = (appointment.Status == AppointmentStatus.Booked && next == AppointmentStatus.Ongoing) ||
                    (appointment.Status == AppointmentStatus.Ongoing && next == AppointmentStatus.Completed);
        if (!valid) return BadRequest(new { message = "Valid lifecycle is Booked → Ongoing → Completed." });
        appointment.Status = next;
        await db.SaveChangesAsync();
        return Ok(new { message = $"Appointment is now {next}." });
    }

    [Authorize(Roles = "Patient")]
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewRequest request)
    {
        if (request.Rating is < 1 or > 5) return BadRequest(new { message = "Rating must be 1-5." });
        var appointment = await db.Appointments.Include(x => x.Review).Include(x => x.Schedule).ThenInclude(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.PatientUserId == User.UserId());
        if (appointment is null || appointment.Status != AppointmentStatus.Completed) return BadRequest(new { message = "Only completed appointments can be reviewed." });
        if (appointment.Review is not null) return Conflict(new { message = "Appointment already reviewed." });

        appointment.Review = new Review { Rating = request.Rating, Comment = request.Comment };
        await db.SaveChangesAsync();

        var docId = appointment.Schedule.DoctorProfileId;
        var stats = await db.Reviews.Where(x => x.Appointment.Schedule.DoctorProfileId == docId).GroupBy(_ => 1)
            .Select(g => new { Avg = g.Average(x => x.Rating), Count = g.Count() }).FirstAsync();
        appointment.Schedule.DoctorProfile.AverageRating = stats.Avg;
        appointment.Schedule.DoctorProfile.ReviewCount = stats.Count;
        await db.SaveChangesAsync();
        return Ok(new { message = "Review submitted." });
    }
}
