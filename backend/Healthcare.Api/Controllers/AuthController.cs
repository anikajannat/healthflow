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
[Route("api/v1/auth")]
public class AuthController(AppDbContext db, JwtService jwt, IEmailService email, IConfiguration config) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(SendOtpRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        if (!Enum.TryParse<OtpPurpose>(request.Purpose, true, out var purpose))
            return BadRequest(new { message = "Purpose must be Registration, DoctorApplication, or PasswordReset." });

        var code = Random.Shared.Next(100000, 999999).ToString();
        db.EmailOtps.Add(new EmailOtp { Email = address, Code = code, Purpose = purpose, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await db.SaveChangesAsync();
        await email.SendAsync(address, "HealthFlow verification code", $"Your verification code is {code}. It expires in 10 minutes.");
        return Ok(new { message = "OTP sent. In local development without SMTP, read the API console log." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        if (request.Password.Length < 8) return BadRequest(new { message = "Password must be at least 8 characters." });
        if (await db.Users.AnyAsync(x => x.Email == address)) return Conflict(new { message = "Email is already registered." });

        var otp = await db.EmailOtps.Where(x => x.Email == address && x.Code == request.Otp &&
                x.Purpose == OtpPurpose.Registration && !x.Used && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
        if (otp is null) return BadRequest(new { message = "Invalid or expired OTP." });

        var user = new User
        {
            Name = request.Name.Trim(), Email = address, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailVerified = true, Role = UserRole.Patient, Status = UserStatus.Active,
            PatientProfile = new PatientProfile()
        };
        db.Users.Add(user);
        otp.Used = true;
        await db.SaveChangesAsync();

        try
        {
            await email.SendAsync(address, "Welcome to HealthFlow", $"Welcome {user.Name}! Your patient account is ready.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Welcome email failed: {ex.Message}");
        }
        return Ok(await IssueTokens(user));
    }

    [HttpPost("doctor-apply")]
    public async Task<IActionResult> DoctorApply(DoctorApplyRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == address)) return Conflict(new { message = "Email is already registered." });
        if (await db.DoctorProfiles.AnyAsync(x => x.LicenseNumber == request.LicenseNumber)) return Conflict(new { message = "License number already exists." });
        if (!await ValidOtp(address, request.Otp, OtpPurpose.DoctorApplication)) return BadRequest(new { message = "Invalid or expired OTP." });

        var user = new User
        {
            Name = request.Name.Trim(), Email = address, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailVerified = true, Role = UserRole.Doctor, Status = UserStatus.Pending,
            DoctorProfile = new DoctorProfile
            {
                LicenseNumber = request.LicenseNumber.Trim(), Specialization = request.Specialization.Trim(),
                Qualifications = request.Qualifications, ExperienceYears = request.ExperienceYears,
                ConsultationFee = request.ConsultationFee, Bio = request.Bio,
                ApprovalStatus = DoctorApprovalStatus.Pending
            }
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { message = "Application submitted. You can log in after admin approval." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(x => x.DoctorProfile).FirstOrDefaultAsync(x => x.Email == address && !x.IsDeleted);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });
        if (user.Status != UserStatus.Active) return StatusCode(403, new { message = $"Account is {user.Status}." });
        return Ok(await IssueTokens(user));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken && x.RefreshTokenExpiresAt > DateTime.UtcNow && !x.IsDeleted);
        if (user is null || user.Status != UserStatus.Active) return Unauthorized(new { message = "Invalid refresh token." });
        return Ok(await IssueTokens(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.UserId();
        var user = await db.Users.Include(x => x.PatientProfile).Include(x => x.DoctorProfile).FirstAsync(x => x.Id == id);
        return Ok(ToUser(user));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        if (!await db.Users.AnyAsync(x => x.Email == address)) return Ok(new { message = "If the account exists, an OTP has been sent." });
        var code = Random.Shared.Next(100000, 999999).ToString();
        db.EmailOtps.Add(new EmailOtp { Email = address, Code = code, Purpose = OtpPurpose.PasswordReset, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await db.SaveChangesAsync();
        await email.SendAsync(address, "Password reset code", $"Your password reset code is {code}.");
        return Ok(new { message = "If the account exists, an OTP has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var address = request.Email.Trim().ToLowerInvariant();
        if (!await ValidOtp(address, request.Otp, OtpPurpose.PasswordReset)) return BadRequest(new { message = "Invalid or expired OTP." });
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == address);
        if (user is null) return NotFound(new { message = "Account not found." });
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.NeedPasswordChange = false;
        user.RefreshToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Password updated." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await db.Users.FirstAsync(x => x.Id == User.UserId());
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.NeedPasswordChange = false;
        user.RefreshToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { message = "Password changed." });
    }

    private async Task<bool> ValidOtp(string emailAddress, string code, OtpPurpose purpose)
    {
        var otp = await db.EmailOtps.Where(x => x.Email == emailAddress && x.Code == code && x.Purpose == purpose && !x.Used && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
        if (otp is null) return false;
        otp.Used = true;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<AuthResponse> IssueTokens(User user)
    {
        var (access, expiresAt) = jwt.CreateAccessToken(user);
        var refresh = jwt.CreateRefreshToken();
        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(int.TryParse(config["Jwt:RefreshDays"], out var d) ? d : 7);
        await db.SaveChangesAsync();
        return new AuthResponse(access, refresh, expiresAt, ToUser(user));
    }

    private static object ToUser(User u) => new
    {
        u.Id, u.Name, u.Email, role = u.Role.ToString(), status = u.Status.ToString(),
        u.NeedPasswordChange, u.ImageUrl,
        patientProfile = u.PatientProfile is null ? null : new
        {
            u.PatientProfile.Id, u.PatientProfile.UserId, u.PatientProfile.Phone, u.PatientProfile.DateOfBirth,
            u.PatientProfile.Gender, u.PatientProfile.Address, u.PatientProfile.BloodGroup,
            u.PatientProfile.Allergies, u.PatientProfile.CurrentMedications, u.PatientProfile.EmergencyContact
        },
        doctorProfile = u.DoctorProfile is null ? null : new
        {
            u.DoctorProfile.Id, u.DoctorProfile.UserId, u.DoctorProfile.LicenseNumber, u.DoctorProfile.Specialization,
            u.DoctorProfile.Qualifications, u.DoctorProfile.ExperienceYears, u.DoctorProfile.ConsultationFee,
            u.DoctorProfile.Bio, approvalStatus = u.DoctorProfile.ApprovalStatus.ToString(),
            u.DoctorProfile.AverageRating, u.DoctorProfile.ReviewCount
        }
    };
}
