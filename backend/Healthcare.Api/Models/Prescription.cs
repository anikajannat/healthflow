namespace Healthcare.Api.Models;

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string Findings { get; set; } = "";
    public string Medicines { get; set; } = "";
    public string? Advice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
