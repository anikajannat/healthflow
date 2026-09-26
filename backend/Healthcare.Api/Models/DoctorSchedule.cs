namespace Healthcare.Api.Models;

public class DoctorSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorProfileId { get; set; }
    public DoctorProfile DoctorProfile { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string MeetLink { get; set; } = "";
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = [];
}
