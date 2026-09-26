namespace Healthcare.Api.Models;

public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string LicenseNumber { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string? Qualifications { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Bio { get; set; }
    public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.Pending;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public ICollection<DoctorSchedule> Schedules { get; set; } = [];
}
