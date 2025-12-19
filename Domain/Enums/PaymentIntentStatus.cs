namespace Domain.Enums;

public enum PaymentIntentStatus
{
    RequiresPaymentMethod, // Needs payment method attached
    RequiresConfirmation, // Frontend needs to confirm
    RequiresAction, // User needs to complete 3D Secure
    Processing, // Stripe is processing
    RequiresCapture, // Authorized successfully (held)
    Succeeded, // Captured successfully
    Canceled
}