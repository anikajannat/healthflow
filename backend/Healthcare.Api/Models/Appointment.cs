namespace Healthcare.Api.Models;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ScheduleId { get; set; }
    public DoctorSchedule Schedule { get; set; } = null!;
    public Guid PatientUserId { get; set; }
    public User PatientUser { get; set; } = null!;
    public int SlotIndex { get; set; }
    public int SerialNumber { get; set; }
    public DateTime AppointmentStartUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public Payment? Payment { get; set; }
    public Prescription? Prescription { get; set; }
    public Review? Review { get; set; }
}
