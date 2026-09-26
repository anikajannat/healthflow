namespace Healthcare.Api.Services;

public record BkashPaymentResult(bool Success, string TransactionId, string Message, string Mode);

public class BkashPaymentService(IConfiguration config)
{
    public Task<BkashPaymentResult> PayAsync(string? mobileNumber, decimal amount, Guid appointmentId)
    {
        var mode = config["Payment:Mode"] ?? "Mock";
        var normalized = (mobileNumber ?? "").Trim().Replace(" ", "").Replace("-", "");

        if (normalized.Length != 11 || !normalized.StartsWith("01") || !normalized.All(char.IsDigit))
            return Task.FromResult(new BkashPaymentResult(false, "", "Enter a valid 11-digit bKash mobile number.", mode));

        // Demo/sandbox-ready flow. Real bKash Checkout requires merchant credentials and their approved API contract.
        if (!string.Equals(mode, "Mock", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new BkashPaymentResult(false, "", "Live bKash is not enabled. Add approved merchant credentials and gateway integration first.", mode));

        var txn = $"BKASH-MOCK-{DateTime.UtcNow:yyMMddHHmm}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();
        return Task.FromResult(new BkashPaymentResult(true, txn, $"Mock bKash payment of BDT {amount:0.00} completed.", mode));
    }
}
