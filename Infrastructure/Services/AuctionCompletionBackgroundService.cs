using Domain.Configs;
using Domain.DTOs.Notifications;
using Domain.Entities;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AuctionCompletionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionCompletionBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
    private const int MaxCaptureAttempts = 3;

    public AuctionCompletionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AuctionCompletionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Completion Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEndedAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Auction Completion Background Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessEndedAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var auctionListingRepository = scope.ServiceProvider.GetRequiredService<IListingRepository<AuctionListing>>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var bidRepository = scope.ServiceProvider.GetRequiredService<IBidRepository>();
        var stripeConnectService = scope.ServiceProvider.GetRequiredService<IStripeConnectService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var paymentSettings = scope.ServiceProvider.GetRequiredService<IOptions<PaymentSettings>>();

        var endedAuctions = await auctionListingRepository.GetEndedAuctionsNotProcessedAsync(DateTime.UtcNow);

        if (endedAuctions.Count == 0)
        {
            _logger.LogDebug("No ended auctions to process");
            return;
        }

        _logger.LogInformation($"Processing {endedAuctions.Count} ended auctions");

        foreach (var auction in endedAuctions)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessAuctionCompletionAsync(
                auction,
                bidRepository,
                paymentRepository,
                auctionListingRepository,
                stripeConnectService,
                notificationService,
                cacheService,
                unitOfWork,
                paymentSettings.Value);
        }
    }

    private async Task ProcessAuctionCompletionAsync(
        AuctionListing auction,
        IBidRepository bidRepository,
        IPaymentRepository paymentRepository,
        IListingRepository<AuctionListing> auctionListingRepository,
        IStripeConnectService stripeConnectService,
        INotificationService notificationService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        PaymentSettings paymentSettings)
    {
        _logger.LogInformation($"Processing auction completion for Auction ID: {auction.Id}, Name: {auction.Name}");

        // Get all bids sorted by price (highest first)
        var bids = await bidRepository.GetAuctionBidsSortedByPrice(auction.Id);

        if (bids.Count == 0)
        {
            _logger.LogWarning($"Auction {auction.Id} has no bids - marking as ended without sale");
            auction.IsArchived = true;
            auctionListingRepository.UpdateListing(auction);
            await unitOfWork.SaveChangesAsync();
            return;
        }

        foreach (var bid in bids)
        {
            if (bid.Payment == null)
            {
                _logger.LogWarning($"Bid {bid.Id} has no associated payment - skipping");
                continue;
            }

            if (bid.Payment.PaymentIntentStatus == PaymentIntentStatus.Canceled ||
                bid.Payment.PaymentIntentStatus == PaymentIntentStatus.Succeeded ||
                bid.Payment.Status == PaymentStatus.Failed)
            {
                _logger.LogDebug(
                    $"Skipping bid {bid.Id} - PaymentIntentStatus: {bid.Payment.PaymentIntentStatus}, Status: {bid.Payment.Status}");
                continue;
            }

            var captureResult = await TryCapturePaymentWithRetry(
                bid,
                stripeConnectService,
                paymentRepository,
                unitOfWork,
                paymentSettings);

            if (captureResult.Success)
            {
                await CancelBidsAsync(bids, excludeBidId: bid.Id, stripeConnectService, paymentRepository, unitOfWork);
                await FinalizeAuctionAsync(
                    auction,
                    bid,
                    captureResult.Payment!,
                    auctionListingRepository,
                    notificationService,
                    cacheService,
                    unitOfWork);

                return; // Auction successfully completed
            }


            // Capture failed - continue to next bidder
            _logger.LogWarning($"Failed to capture payment for bid {bid.Id} after retries - trying next bidder");
        }

        await CancelBidsAsync(bids, excludeBidId: null, stripeConnectService, paymentRepository, unitOfWork);
        _logger.LogError($"All bids failed for auction {auction.Id} - no successful payment capture");
        auction.IsArchived = true;
        auctionListingRepository.UpdateListing(auction);
        await unitOfWork.SaveChangesAsync();

        // Notify seller
        await notificationService.SendNotification(
            [auction.OwnerId],
            0, // System notification
            new NotificationInfo
            {
                Message =
                    $"Your auction '{auction.Name}' ended but no payment could be processed. Please contact support.",
                ListingId = auction.Id,
                ResourceType = ResourceType.Auction.ToString(),
                NotificationImageUrl = auction.ListingPhotos.FirstOrDefault(lp => lp.Order == 1)?.Photo.Url
            });
    }

    private async Task<(bool Success, Payment? Payment)> TryCapturePaymentWithRetry(
        AuctionBid bid,
        IStripeConnectService stripeConnectService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        PaymentSettings paymentSettings)
    {
        var payment = bid.Payment!;

        for (int attemptCount = 1; attemptCount <= MaxCaptureAttempts; attemptCount++)
        {
            try
            {
                _logger.LogInformation(
                    $"Attempting to capture PaymentIntent for Bid {bid.Id}, " +
                    $"PaymentIntent: {payment.StripePaymentIntentId}, " +
                    $"Amount: {payment.TotalAmount} {payment.Currency.ToUpper()}, " +
                    $"Attempt: {attemptCount}/{MaxCaptureAttempts}");

                // Capture the PaymentIntent
                var capturedIntent = await stripeConnectService.CapturePaymentIntentAsync(
                    payment.StripePaymentIntentId!);

                // Update payment status
                payment.StripeChargeId = capturedIntent.LatestChargeId;
                payment.Status = PaymentStatus.Pending; // Now waiting for transfer to seller
                payment.PaymentIntentStatus = PaymentIntentStatusExtensions.FromStripeStatus(capturedIntent.Status);
                payment.TransferTrigger = TransferTrigger.TimeBased;
                payment.ScheduledTransferDate = DateTime.UtcNow.AddDays(paymentSettings.TransferDelayDays);
                payment.ChargeDate = DateTime.UtcNow;
                payment.Notes = $"Payment captured successfully on attempt {attemptCount}";

                paymentRepository.Update(payment);
                await unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    $"Successfully captured payment for Bid {bid.Id}, PaymentIntent: {payment.StripePaymentIntentId}");

                return (true, payment); // SUCCESS - exit immediately
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    $"Capture attempt {attemptCount}/{MaxCaptureAttempts} failed for Bid {bid.Id}. Error: {ex.Message}");

                if (attemptCount < MaxCaptureAttempts)
                {
                    // Not the last attempt - wait and retry
                    var delaySeconds = 10 * attemptCount; 
                    _logger.LogInformation($"Waiting {delaySeconds} seconds before retry...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    // Final failure after all retries
                    payment.Status = PaymentStatus.Failed;
                    payment.Notes = $"Failed to capture after {MaxCaptureAttempts} attempts. Last error: {ex.Message}";
                    paymentRepository.Update(payment);
                    await unitOfWork.SaveChangesAsync();

                    _logger.LogError(ex,
                        $"Payment capture FAILED for Bid {bid.Id} after {MaxCaptureAttempts} attempts");

                    return (false, null); // FAILURE - all attempts exhausted
                }
            }
        }

        // Should never reach here, but added for completeness
        return (false, null);
    }


    private async Task FinalizeAuctionAsync(
        AuctionListing auction,
        AuctionBid winningBid,
        Payment payment,
        IListingRepository<AuctionListing> auctionListingRepository,
        INotificationService notificationService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _logger.LogInformation(
            $"Finalizing auction {auction.Id} - Winner: User {winningBid.BidderId}, Amount: {winningBid.BidPrice}");

        // Mark auction as sold
        auction.IsArchived = true;

        auctionListingRepository.UpdateListing(auction);
        await unitOfWork.SaveChangesAsync();

        // Clear cache
        await cacheService.RemoveAsync($"{Domain.Configs.CacheKeys.AUCTION_LISTING}{auction.Id}");
        await cacheService.RemoveAsync($"{Domain.Configs.CacheKeys.BIDS}{auction.Id}");
        await cacheService.RemoveByPatternAsync($"{Domain.Configs.CacheKeys.LISTINGS}*");

        // Notify winner
        await notificationService.SendNotification(
            [winningBid.BidderId],
            0, // System notification
            new NotificationInfo
            {
                Message =
                    $"Congratulations! You won the auction '{auction.Name}' with a bid of {winningBid.BidPrice} PLN. " +
                    $"Payment has been processed and funds will be transferred to the seller within {payment.ScheduledTransferDate?.Subtract(DateTime.UtcNow).Days ?? 0} days.",
                ListingId = auction.Id,
                ResourceType = ResourceType.Auction.ToString(),
                NotificationImageUrl = auction.ListingPhotos.FirstOrDefault(lp => lp.Order == 1)?.Photo.Url
            });

        // Notify seller
        await notificationService.SendNotification(
            [auction.OwnerId],
            0, // System notification
            new NotificationInfo
            {
                Message = $"Your auction '{auction.Name}' has ended! Winner: {winningBid.Bidder.UserName}, " +
                          $"Final bid: {winningBid.BidPrice} PLN. You will receive {payment.SellerAmount} PLN " +
                          $"(after {payment.PlatformFeePercentage}% platform fee).",
                ListingId = auction.Id,
                ResourceType = ResourceType.Auction.ToString(),
                NotificationImageUrl = auction.ListingPhotos.FirstOrDefault(lp => lp.Order == 1)?.Photo.Url
            });

        _logger.LogInformation($"Auction {auction.Id} finalized successfully");
    }

    private async Task CancelBidsAsync(
        List<AuctionBid> allBids,
        int? excludeBidId, // null = cancel all, value = cancel all except this one
        IStripeConnectService stripeConnectService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        foreach (var bid in allBids)
        {
            if (excludeBidId.HasValue && bid.Id == excludeBidId.Value)
                continue;

            if (bid.Payment == null)
                continue;

            if (!string.IsNullOrEmpty(bid.Payment.StripePaymentIntentId) &&
                bid.Payment.PaymentIntentStatus != PaymentIntentStatus.Canceled)
            {
                try
                {
                    await stripeConnectService.CancelPaymentIntentAsync(bid.Payment.StripePaymentIntentId);

                    bid.Payment.PaymentIntentStatus = PaymentIntentStatus.Canceled;
                    bid.Payment.Status = excludeBidId.HasValue ? PaymentStatus.Outbid : PaymentStatus.Failed;
                    bid.Payment.Notes = excludeBidId.HasValue
                        ? $"PaymentIntent canceled - auction won by bid {excludeBidId.Value}"
                        : "Auction ended - all payment captures failed";
                    paymentRepository.Update(bid.Payment);

                    _logger.LogInformation($"Canceled PaymentIntent for bid {bid.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to cancel PaymentIntent for bid {bid.Id}");
                }
            }
        }

        await unitOfWork.SaveChangesAsync();
    }
}