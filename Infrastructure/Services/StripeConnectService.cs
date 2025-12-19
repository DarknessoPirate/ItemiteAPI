using System.Linq.Expressions;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Stripe;
using StripeException = Stripe.StripeException;

namespace Infrastructure.Services;

public class StripeConnectService(
    UserManager<User> userManager,
    ILogger<StripeConnectService> logger,
    IUnitOfWork unitOfWork
) : IStripeConnectService
{
    /// <summary>
    /// Creates a charge on the buyer's card to the platform account (marketplace account)
    /// </summary>
    /// <param name="amount">Amount in currency (e.g., 100.50)</param>
    /// <param name="currency">Currency code (e.g., "pln")</param>
    /// <param name="paymentMethodId">Payment method ID from frontend (card token)</param>
    /// <param name="description">Description of the charge</param>
    /// <param name="metadata">Additional data to store with the charge</param>
    /// <returns>The created Charge object from Stripe</returns>
    public async Task<Charge> CreateChargeAsync(
        decimal amount,
        string currency,
        string paymentMethodId,
        string description,
        Dictionary<string, string>? metadata = null)
    {
        var chargeService = new ChargeService();
        // stripe uses smallest possible unit
        var amountInSmallestUnit = (long)(amount * 100);
        var chargeOptions = new ChargeCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = currency.ToLower(),
            Source = paymentMethodId,
            Description = description,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        try
        {
            var charge = await chargeService.CreateAsync(chargeOptions);
            return charge;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe charge failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe charge failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Creates a PaymentIntent to authorize (hold) funds for an auction bid
    /// </summary>
    /// <param name="amount">Amount to authorize</param>
    /// <param name="currency">Currency code</param>
    /// <param name="paymentMethodId">Payment method ID from frontend</param>
    /// <param name="description">Description</param>
    /// <param name="metadata">Metadata</param>
    /// <returns>Created PaymentIntent</returns>
    public async Task<PaymentIntent> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string paymentMethodId,
        string description,
        Dictionary<string, string>? metadata = null)
    {
        var paymentIntentService = new PaymentIntentService();
        var amountInSmallestUnit = (long)(amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = currency.ToLower(),
            PaymentMethod = paymentMethodId,
            Description = description,
            Metadata = metadata ?? new Dictionary<string, string>(),
            CaptureMethod = "manual", // Don't charge immediately - only authorize
            Confirm = true, // Confirm immediately to check if card is valid
            ReturnUrl = "https://your-frontend-url.com/auction-bid-complete", // For 3D Secure redirects
        };

        try
        {
            var paymentIntent = await paymentIntentService.CreateAsync(options);
            return paymentIntent;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe PaymentIntent creation failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe PaymentIntent creation failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Captures an authorized PaymentIntent (actually charges the card)
    /// </summary>
    /// <param name="paymentIntentId">The PaymentIntent ID to capture</param>
    /// <param name="amountToCapture">Optional: capture partial amount (null for full)</param>
    /// <returns>Updated PaymentIntent</returns>
    public async Task<PaymentIntent> CapturePaymentIntentAsync(
        string paymentIntentId,
        decimal? amountToCapture = null)
    {
        var paymentIntentService = new PaymentIntentService();

        var options = new PaymentIntentCaptureOptions();

        if (amountToCapture.HasValue)
        {
            options.AmountToCapture = (long)(amountToCapture.Value * 100);
        }

        try
        {
            var paymentIntent = await paymentIntentService.CaptureAsync(paymentIntentId, options);
            return paymentIntent;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe PaymentIntent capture failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe PaymentIntent capture failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Cancels a PaymentIntent (releases the hold) - used when user is outbid
    /// </summary>
    /// <param name="paymentIntentId">The PaymentIntent ID to cancel</param>
    /// <returns>Canceled PaymentIntent</returns>
    public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentService = new PaymentIntentService();

        try
        {
            var paymentIntent = await paymentIntentService.CancelAsync(paymentIntentId);
            return paymentIntent;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe PaymentIntent cancellation failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe PaymentIntent cancellation failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Gets a PaymentIntent by ID to check its status
    /// </summary>
    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentService = new PaymentIntentService();

        try
        {
            return await paymentIntentService.GetAsync(paymentIntentId);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Failed to retrieve PaymentIntent. Error: {ErrorMessage}",
                ex.StripeError?.Message);
            throw new StripeErrorException($"Failed to retrieve PaymentIntent: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Transfers money from the platform account to seller's Stripe Connect account.
    /// </summary>
    /// <param name="amount">Amount to transfer in chosen currency (e.g., 95.00)</param>
    /// <param name="currency">Currency code (e.g., "pln")</param>
    /// <param name="destinationAccountId">Seller's Stripe Connect Account ID</param>
    /// <param name="description">Description of the transfer</param>
    /// <param name="metadata">Additional data to store with the transfer</param>
    /// <returns>The created Transfer object from Stripe</returns>
    public async Task<Transfer> CreateTransferAsync(
        decimal amount,
        string currency,
        string destinationAccountId,
        string description,
        Dictionary<string, string>? metadata = null)
    {
        var transferService = new TransferService();
        var amountInSmallestUnit = (long)(amount * 100);
        var transferOptions = new TransferCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = currency.ToLower(),
            Destination = destinationAccountId,
            Description = description,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        try
        {
            var transfer = await transferService.CreateAsync(transferOptions);
            return transfer;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe transfer failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe transfer failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }

    /// <summary>
    /// Creates a refund for a charge. It is marked as a refund in the system and transfer fees don't apply.
    /// </summary>
    /// <param name="chargeId">The Stripe charge ID to refund</param>
    /// <param name="amount">Amount to refund (null for full refund)</param>
    /// <param name="reason">Reason for refund</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>The created Refund object</returns>
    public async Task<Refund> CreateRefundAsync(string chargeId, decimal? amount = null, string? reason = null,
        Dictionary<string, string>? metadata = null)
    {
        var refundService = new RefundService();
        var refundOptions = new RefundCreateOptions
        {
            Charge = chargeId,
            Reason = reason,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        if (amount.HasValue)
        {
            refundOptions.Amount = (long)(amount.Value * 100);
        }

        try
        {
            var refund = await refundService.CreateAsync(refundOptions);
            return refund;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe refund failed. Error: {ErrorMessage}, Code: {ErrorCode}",
                ex.StripeError?.Message,
                ex.StripeError?.Code);
            throw new StripeErrorException($"Stripe refund failed: {ex.StripeError?.Message}",
                detailedMessage: ex.StripeError?.Message);
        }
    }


    /// <summary>
    /// Creates a user's (seller) stripe connect account and generates a link to stripe onboarding 
    /// </summary>
    /// <param name="user">user entity to get details (email) and save stripe account id</param>
    /// <param name="returnUrl">Url to which stripe will redirect after a successful onboarding</param>
    /// <param name="refreshUrl"> Url to which stripe will redirect if the onboarding link is exipired (frontend refresh onboarding url)</param>
    /// <returns>The generated onboardinbg link for stripe connect account</returns>
    public async Task<string> CreateConnectAccountAndGetOnboardingUrlAsync(User user, string returnUrl,
        string refreshUrl)
    {
        var accountService = new AccountService();
        var account = await accountService.CreateAsync(new AccountCreateOptions
        {
            // Country = 
            Type = "express",
            Email = user.Email,
            Capabilities = new AccountCapabilitiesOptions
            {
                BlikPayments = new AccountCapabilitiesBlikPaymentsOptions { Requested = true },
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        });

        user.StripeConnectAccountId = account.Id;
        await unitOfWork.SaveChangesAsync();

        return await GenerateOnboardingLinkAsync(account.Id, returnUrl, refreshUrl);
    }

    /// <summary>
    /// Creates a user's (seller) stripe connect account  
    /// </summary>
    /// <param name="stripeAccountId">stripe account id for which the onboarding link will be generated</param>
    /// <param name="returnUrl">Url to which stripe will redirect after a successful onboarding</param>
    /// <param name="refreshUrl"> Url to which stripe will redirect if the onboarding link is exipired (frontend refresh onboarding url)</param>
    /// <returns>The generated onboardinbg link for stripe connect account</returns>
    public async Task<string> GenerateOnboardingLinkAsync(string stripeAccountId, string returnUrl, string refreshUrl)
    {
        var linkService = new AccountLinkService();
        var accountLink = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccountId,
            RefreshUrl = refreshUrl,
            ReturnUrl = returnUrl,
            Type = "account_onboarding"
        });

        return accountLink.Url;
    }

    public async Task<bool> IsAccountFullyOnboardedAsync(string stripeAccountId)
    {
        var accountService = new AccountService();
        var account = await accountService.GetAsync(stripeAccountId);

        return account.ChargesEnabled && account.DetailsSubmitted;
    }
}