namespace Healthcare.Api.DTOs;

public record SendOtpRequest(string Email, string Purpose);
public record RegisterRequest(string Name, string Email, string Password, string Otp);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record DoctorApplyRequest(
    string Name, string Email, string Password, string Otp,
    string LicenseNumber, string Specialization, string? Qualifications,
    int ExperienceYears, decimal ConsultationFee, string? Bio);
public record AuthResponse(
    string AccessToken, string RefreshToken, DateTime ExpiresAt,
    object User);
