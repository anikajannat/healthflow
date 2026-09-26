namespace Healthcare.Api.Models;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Provider { get; set; } = "Mock";
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}
