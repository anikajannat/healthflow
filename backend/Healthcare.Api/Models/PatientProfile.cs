namespace Healthcare.Api.Models;

public class PatientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public string? EmergencyContact { get; set; }
}
