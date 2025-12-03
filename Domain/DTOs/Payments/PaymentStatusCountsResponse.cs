using Domain.Enums;

namespace Domain.DTOs.Payments;

public class PaymentStatusCountsResponse
{
    public Dictionary<PaymentStatus, int> StatusCounts { get; set; } = new();
    public int TotalPayments { get; set; }
}