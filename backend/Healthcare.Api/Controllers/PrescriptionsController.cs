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
[Route("api/v1/prescriptions")]
[Authorize(Roles = "Doctor")]
public class PrescriptionsController(AppDbContext db, IEmailService email, SimplePdfService pdf) : ControllerBase
{
    [HttpPost("appointment/{appointmentId:guid}")]
    public async Task<IActionResult> Create(Guid appointmentId, PrescriptionRequest request)
    {
        var appointment = await db.Appointments.Include(x => x.PatientUser)
            .Include(x => x.Schedule).ThenInclude(x => x.DoctorProfile).ThenInclude(x => x.User)
            .Include(x => x.Prescription)
            .FirstOrDefaultAsync(x => x.Id == appointmentId && x.Schedule.DoctorProfile.UserId == User.UserId());

        if (appointment is null) return NotFound();
        if (appointment.Status != AppointmentStatus.Completed) return BadRequest(new { message = "Prescription requires a completed appointment." });
        if (appointment.Prescription is not null) return Conflict(new { message = "Prescription already exists." });

        appointment.Prescription = new Prescription { Findings = request.Findings, Medicines = request.Medicines, Advice = request.Advice };
        await db.SaveChangesAsync();

        var bytes = pdf.Create("HealthFlow Digital Prescription", new[]
        {
            $"Patient: {appointment.PatientUser.Name}", $"Doctor: {appointment.Schedule.DoctorProfile.User.Name}",
            $"Findings: {request.Findings}", $"Medicines: {request.Medicines}", $"Advice: {request.Advice ?? "-"}"
        });
        await email.SendAsync(appointment.PatientUser.Email, "Your prescription", "Your digital prescription is attached.", bytes, "prescription.pdf");
        return Ok(new { message = "Prescription created and emailed." });
    }
}
