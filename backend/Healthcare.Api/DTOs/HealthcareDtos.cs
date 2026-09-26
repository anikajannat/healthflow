namespace Healthcare.Api.DTOs;

public record UpdatePatientProfileRequest(
    string? Phone, DateOnly? DateOfBirth, string? Gender, string? Address,
    string? BloodGroup, string? Allergies, string? CurrentMedications, string? EmergencyContact);

public record CreateScheduleRequest(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string MeetLink);
public record UpdateScheduleRequest(TimeOnly? StartTime, TimeOnly? EndTime, string? MeetLink, string? Status);

public record BookAppointmentRequest(Guid ScheduleId, int SlotIndex, string? Reason, string? PaymentMethod, string? BkashNumber);
public record PrescriptionRequest(string Findings, string Medicines, string? Advice);
public record ReviewRequest(int Rating, string? Comment);

public record CreateAdminRequest(string Name, string OrganizationEmail, string PersonalEmail, string Role);
