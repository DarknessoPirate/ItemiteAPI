using Domain.Enums;

namespace Domain.Extensions;
public static class PaymentIntentStatusExtensions
{
    public static PaymentIntentStatus FromStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentIntentStatus.RequiresPaymentMethod,
            "requires_confirmation" => PaymentIntentStatus.RequiresConfirmation,
            "requires_action" => PaymentIntentStatus.RequiresAction,
            "processing" => PaymentIntentStatus.Processing,
            "requires_capture" => PaymentIntentStatus.RequiresCapture,
            "succeeded" => PaymentIntentStatus.Succeeded,
            "canceled" => PaymentIntentStatus.Canceled,
            _ => PaymentIntentStatus.RequiresAction
        };
    }
}
