using Domain.Entities;
using Stripe;

namespace Infrastructure.Interfaces.Services;

public interface IStripeConnectService
{
    Task<string> CreateConnectAccountAndGetOnboardingUrlAsync(User user, string returnUrl, string refreshUrl);
    Task<string> GenerateOnboardingLinkAsync(string accountId, string returnUrl, string refreshUrl);
    Task<bool> IsAccountFullyOnboardedAsync(string stripeAccountId);

    Task<Charge> CreateChargeAsync(
        decimal amount,
        string currency,
        string paymentMethodId,
        string description,
        Dictionary<string, string>? metadata = null);

    Task<PaymentIntent> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string paymentMethodId,
        string description,
        Dictionary<string, string>? metadata = null);

    Task<PaymentIntent> CapturePaymentIntentAsync(
        string paymentIntentId,
        decimal? amountToCapture = null);

    Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId);

    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

    Task<Transfer> CreateTransferAsync(
        decimal amount,
        string currency,
        string destinationAccountId,
        string description,
        Dictionary<string, string>? metadata = null);

    Task<Refund> CreateRefundAsync(
        string chargeId,
        decimal? amount = null,
        string? reason = null,
        Dictionary<string, string>? metadata = null);
}