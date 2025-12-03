using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class PaymentSettings
{
    [Range(0.01, 100, ErrorMessage = "PaymentSettings:PlatformFeePercentage must be between 0.01 and 100 in appsettings")]
    public decimal PlatformFeePercentage { get; set; }

    [Range(1, 30, ErrorMessage = "PaymentSettings:TransferDelayDays must be in range <1,30> in appsettings")]
    public int TransferDelayDays { get; set; }
    
    public int DisputeTimeWindowInDays { get; set; }
}