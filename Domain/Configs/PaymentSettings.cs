using System.ComponentModel.DataAnnotations;

namespace Domain.Configs;

public class PaymentSettings
{
    [Range(0.01, 100,
        ErrorMessage = "PaymentSettings:PlatformFeePercentage must be between 0.01 and 100 in appsettings")]
    public decimal PlatformFeePercentage { get; set; }

    [Range(1, 30, ErrorMessage = "PaymentSettings:TransferDelayDays must be in range <1,30> in appsettings")]
    public int TransferDelayDays { get; set; }

    [Required(ErrorMessage = "PaymentSettings:DisputeTimeWindowInDays field is required in appsettings")]
    public int DisputeTimeWindowInDays { get; set; }

    [Required(ErrorMessage = "PaymentSettings:BidCOmpleteUrl field is required in appsettings")]
    public string BidCompleteUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "PaymentSettings:PurchaseCompleteUrl field is required in appsettings")]
    public string PurchaseCompleteUrl { get; set; } = string.Empty;
}