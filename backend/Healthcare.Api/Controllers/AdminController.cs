using Healthcare.Api.Data;
using Healthcare.Api.DTOs;
using Healthcare.Api.Helpers;
using Healthcare.Api.Models;
using Healthcare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController(AppDbContext db, IEmailService email) : ControllerBase
{
    [HttpGet("doctor-applications")]
    public async Task<IActionResult> DoctorApplications()
    {
        var items = await db.DoctorProfiles.AsNoTracking().Include(x => x.User)
            .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Pending)
            .Select(x => new { x.Id, x.User.Name, x.User.Email, x.LicenseNumber, x.Specialization, x.Qualifications, x.ExperienceYears, x.ConsultationFee, x.Bio })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("doctors/{doctorProfileId:guid}/decision/{decision}")]
    public async Task<IActionResult> DoctorDecision(Guid doctorProfileId, string decision)
    {
        var doctor = await db.DoctorProfiles.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == doctorProfileId);
        if (doctor is null) return NotFound();

        if (decision.Equals("approve", StringComparison.OrdinalIgnoreCase))
        {
            doctor.ApprovalStatus = DoctorApprovalStatus.Approved;
            doctor.User.Status = UserStatus.Active;
            await db.SaveChangesAsync();
            await email.SendAsync(doctor.User.Email, "Doctor application approved", $"Welcome Dr. {doctor.User.Name}. Your HealthFlow doctor account is active.");
            return Ok(new { message = "Doctor approved." });
        }
        if (decision.Equals("reject", StringComparison.OrdinalIgnoreCase))
        {
            doctor.ApprovalStatus = DoctorApprovalStatus.Rejected;
            doctor.User.Status = UserStatus.Rejected;
            await db.SaveChangesAsync();
            return Ok(new { message = "Doctor rejected." });
        }
        return BadRequest(new { message = "Decision must be approve or reject." });
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? role)
    {
        var q = db.Users.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsed)) q = q.Where(x => x.Role == parsed);
        return Ok(await q.OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Name, x.Email, role = x.Role.ToString(), status = x.Status.ToString(), x.EmailVerified, x.CreatedAt }).ToListAsync());
    }

    [HttpPost("users/{id:guid}/block/{blocked:bool}")]
    public async Task<IActionResult> Block(Guid id, bool blocked)
    {
        var target = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (target is null) return NotFound();

        var callerRole = User.Role();
        if ((target.Role == UserRole.Admin || target.Role == UserRole.SuperAdmin) && callerRole != UserRole.SuperAdmin.ToString())
            return StatusCode(403, new { message = "Only a Super Admin can manage Admin/SuperAdmin accounts." });

        target.Status = blocked ? UserStatus.Blocked : UserStatus.Active;
        target.RefreshToken = null;
        await db.SaveChangesAsync();
        return Ok(new { message = blocked ? "User blocked." : "User unblocked." });
    }

    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin(CreateAdminRequest request)
    {
        var role = request.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ? UserRole.SuperAdmin : UserRole.Admin;
        if (role == UserRole.SuperAdmin && User.Role() != UserRole.SuperAdmin.ToString())
            return StatusCode(403, new { message = "Only Super Admin can create another Super Admin." });

        var loginEmail = request.OrganizationEmail.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == loginEmail)) return Conflict(new { message = "Organization email already exists." });

        var tempPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + "aA1!";
        db.Users.Add(new User
        {
            Name = request.Name.Trim(), Email = loginEmail, PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            EmailVerified = true, Role = role, Status = UserStatus.Active, NeedPasswordChange = true
        });
        await db.SaveChangesAsync();
        await email.SendAsync(request.PersonalEmail, "Your HealthFlow admin account",
            $"Login email: {loginEmail}\nTemporary password: {tempPassword}\nPlease change this password after login.");
        return Ok(new { message = "Admin account created and credentials emailed to the personal email." });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Ok(new
        {
            patients = await db.Users.CountAsync(x => x.Role == UserRole.Patient && !x.IsDeleted),
            doctors = await db.Users.CountAsync(x => x.Role == UserRole.Doctor && x.Status == UserStatus.Active && !x.IsDeleted),
            pendingDoctors = await db.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Pending),
            appointments = await db.Appointments.CountAsync(),
            todaySchedules = await db.DoctorSchedules.CountAsync(x => x.Date == today),
            revenue = await db.Payments.Where(x => x.Status == PaymentStatus.Paid).SumAsync(x => (decimal?)x.Amount) ?? 0
        });
    }
}
