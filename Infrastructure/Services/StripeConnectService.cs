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
            logger.LogError("Stripe charge failed: {}", ex.StripeError.Message);
            throw new StripeErrorException($"Stripe charge failed", detailedMessage: ex.StripeError.Message);
        }
    }


    /// <summary>
    /// Transfers money from the platform account to seller's Stripe Connect account
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
            logger.LogError("Stripe transfer failed: {}", ex.StripeError.Message);
            throw new StripeErrorException($"Stripe transfer failed", detailedMessage: ex.StripeError.Message);
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