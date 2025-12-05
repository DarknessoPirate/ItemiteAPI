using Domain.Entities;
using Domain.Enums;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PaymentTransferBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentTransferBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);
    private const int MaxRetryAttempts = 3;

    public PaymentTransferBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PaymentTransferBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment transfer background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingTransfersAsync(stoppingToken);
                await ProcessScheduledRefundsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Payment Transfer Background Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingTransfersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var stripeConnectService = scope.ServiceProvider.GetRequiredService<IStripeConnectService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingPayments = await paymentRepository.FindAllPendingAsync();
        if (pendingPayments.Count == 0)
        {
            _logger.LogDebug("No pending payments to process. Exiting.");
            return;
        }

        _logger.LogInformation($"Processing {pendingPayments.Count} pending payments");
        foreach (var payment in pendingPayments)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessTransferAsync(payment, stripeConnectService, paymentRepository, unitOfWork);
        }
    }

    private async Task ProcessTransferAsync(Payment payment,
        IStripeConnectService stripeConnectService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        payment.TransferAttempts += 1;

        try
        {
            _logger.LogInformation(
                $"Processing transfer for Payment ID: {payment.Id}, " +
                $"Amount: {payment.SellerAmount} {payment.Currency.ToUpper()}, " +
                $"Seller: {payment.SellerId}, Attempt: {payment.TransferAttempts + 1}, " +
                $"Processing type: ${payment.TransferTrigger.ToString()}"
            );

            var transfer = await stripeConnectService.CreateTransferAsync(
                amount: payment.SellerAmount,
                currency: payment.Currency,
                destinationAccountId: payment.Seller.StripeConnectAccountId,
                description: $"Automatic payment for listing id: {payment.ListingId}:\"{payment.Listing.Name}\"",
                metadata:
                new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "listing_id", payment.ListingId.ToString() },
                    { "seller_id", payment.SellerId.ToString() },
                    { "transfer_trigger_method", payment.TransferTrigger.ToString() }
                }
            );

            payment.StripeTransferId = transfer.Id;
            payment.Status = PaymentStatus.Transferred;
            payment.TransferDate = DateTime.UtcNow;
            payment.ActualTransferMethod = TransferMethod.Automatic;


            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                $"Transfer successful for Payment ID: {payment.Id}, " +
                $"Stripe Transfer ID: {transfer.Id}");
        }
        catch (Exception ex)
        {
            if (payment.TransferAttempts >= MaxRetryAttempts)
            {
                payment.Status = PaymentStatus.Failed;
                payment.Notes = string.IsNullOrEmpty(payment.Notes)
                    ? $"Transfer failed after {MaxRetryAttempts} attempts. Last error: {ex.Message}"
                    : $"{payment.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Transfer failed after {MaxRetryAttempts} attempts. Error: {ex.Message}";

                _logger.LogError(ex,
                    $"Transfer FAILED for Payment ID: {payment.Id} after {MaxRetryAttempts} attempts. " +
                    $"Status changed to Failed.");
            }
            else
            {
                payment.ScheduledTransferDate = DateTime.UtcNow.AddHours(1); // add 1h to scheduled date to retry later
                payment.Notes = string.IsNullOrEmpty(payment.Notes)
                    ? $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Transfer attempt {payment.TransferAttempts} failed with message: {ex.Message}. Retry scheduled."
                    : $"{payment.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Transfer attempt {payment.TransferAttempts} failed: {ex.Message}. Retry scheduled.";

                _logger.LogWarning(ex,
                    $"Transfer attempt {payment.TransferAttempts}/{MaxRetryAttempts} failed for Payment ID: {payment.Id}. " +
                    $"Retry scheduled for {payment.ScheduledTransferDate:yyyy-MM-dd HH:mm:ss}. Error: {ex.Message}");
            }

            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private async Task ProcessScheduledRefundsAsync(CancellationToken cancellationToken)
{
    using var scope = _serviceProvider.CreateScope();
    var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
    var stripeConnectService = scope.ServiceProvider.GetRequiredService<IStripeConnectService>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    var scheduledRefunds = await paymentRepository.FindAllScheduledRefundsAsync();
    if (scheduledRefunds.Count == 0)
    {
        _logger.LogDebug("No scheduled refunds to process.");
        return;
    }

    _logger.LogInformation($"Processing {scheduledRefunds.Count} scheduled refunds");
    foreach (var payment in scheduledRefunds)
    {
        if (cancellationToken.IsCancellationRequested)
            break;
        
        if (payment.Status == PaymentStatus.PartialRefundScheduled)
        {
            await ProcessPartialRefundWithTransferAsync(payment, stripeConnectService, paymentRepository, unitOfWork);
        }
        else if(payment.Status == PaymentStatus.RefundScheduled)
        {
            await ProcessRefundAsync(payment, stripeConnectService, paymentRepository, unitOfWork);
        }
    }
}

    private async Task ProcessRefundAsync(Payment payment,
        IStripeConnectService stripeConnectService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        payment.RefundAttempts += 1;

        try
        {
            _logger.LogInformation(
                $"Processing refund for Payment ID: {payment.Id}, " +
                $"Amount: {payment.RefundAmount} {payment.Currency.ToUpper()}, " +
                $"Buyer: {payment.BuyerId}, Attempt: {payment.RefundAttempts}");

            var refund = await stripeConnectService.CreateRefundAsync(
                chargeId: payment.StripeChargeId,
                amount: payment.RefundAmount,
                reason: "requested_by_customer",
                metadata: new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "listing_id", payment.ListingId.ToString() },
                    { "buyer_id", payment.BuyerId.ToString() },
                    { "refund_type", payment.RefundAmount == payment.TotalAmount ? "full" : "partial" }
                }
            );

            payment.StripeRefundId = refund.Id;
            payment.Status = payment.StripeTransferId != null
                ? PaymentStatus.PartiallyRefunded // Partial refund - already transferred to seller
                : PaymentStatus.Refunded; // Full refund - no transfer
            payment.RefundDate = DateTime.UtcNow;
            payment.Notes = AddNote(payment.Notes,
                $"Refund of {payment.RefundAmount} {payment.Currency.ToUpper()} processed successfully.");

            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                $"Refund successful for Payment ID: {payment.Id}, " +
                $"Stripe Refund ID: {refund.Id}");
        }
        catch (Exception ex)
        {
            if (payment.RefundAttempts >= MaxRetryAttempts)
            {
                payment.Status = PaymentStatus.Failed;
                payment.Notes = AddNote(payment.Notes,
                    $"Refund failed after {MaxRetryAttempts} attempts. Last error: {ex.Message}");

                _logger.LogError(ex,
                    $"Refund FAILED for Payment ID: {payment.Id} after {MaxRetryAttempts} attempts. " +
                    $"Status changed to Failed.");
            }
            else
            {
                payment.ScheduledRefundDate = DateTime.UtcNow.AddHours(1);
                payment.Notes = AddNote(payment.Notes,
                    $"Refund attempt {payment.RefundAttempts} failed: {ex.Message}. Retry scheduled.");

                _logger.LogWarning(ex,
                    $"Refund attempt {payment.RefundAttempts}/{MaxRetryAttempts} failed for Payment ID: {payment.Id}. " +
                    $"Retry scheduled for {payment.ScheduledRefundDate:yyyy-MM-dd HH:mm:ss}. Error: {ex.Message}");
            }

            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
        }
    }
    
    private async Task ProcessPartialRefundWithTransferAsync(Payment payment,
        IStripeConnectService stripeConnectService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        payment.RefundAttempts += 1;
        payment.TransferAttempts += 1;
    
        try
        {
            _logger.LogInformation(
                $"Processing partial refund for Payment ID: {payment.Id}, " +
                $"Refund: {payment.RefundAmount} {payment.Currency.ToUpper()}, " +
                $"Transfer: {payment.SellerAmount} {payment.Currency.ToUpper()}, " +
                $"Attempt: {payment.RefundAttempts}");
    
            // 1. Process refund to buyer
            var refund = await stripeConnectService.CreateRefundAsync(
                chargeId: payment.StripeChargeId,
                amount: payment.RefundAmount,
                reason: "requested_by_customer",
                metadata: new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "listing_id", payment.ListingId.ToString() },
                    { "buyer_id", payment.BuyerId.ToString() },
                    { "refund_type", "partial" }
                }
            );
    
            payment.StripeRefundId = refund.Id;
            payment.RefundDate = DateTime.UtcNow;
    
            _logger.LogInformation($"Refund successful for Payment ID: {payment.Id}, Refund ID: {refund.Id}");
    
            // 2. Process transfer to seller
            var transfer = await stripeConnectService.CreateTransferAsync(
                amount: payment.SellerAmount,
                currency: payment.Currency,
                destinationAccountId: payment.Seller.StripeConnectAccountId,
                description: $"Partial payment for listing #{payment.ListingId} - After partial refund",
                metadata: new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "listing_id", payment.ListingId.ToString() },
                    { "seller_id", payment.SellerId.ToString() },
                    { "resolution", "partial_refund" },
                    { "refund_amount", payment.RefundAmount!.Value.ToString("F2") }
                }
            );
    
            payment.StripeTransferId = transfer.Id;
            payment.TransferDate = DateTime.UtcNow;
            payment.ActualTransferMethod = TransferMethod.Automatic;
            payment.Status = PaymentStatus.PartiallyRefunded;
            payment.Notes = AddNote(payment.Notes,
                $"Partial refund of {payment.RefundAmount} processed and {payment.SellerAmount} transferred to seller.");
    
            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
    
            _logger.LogInformation(
                $"Partial refund complete for Payment ID: {payment.Id}, " +
                $"Refund ID: {refund.Id}, Transfer ID: {transfer.Id}");
        }
        catch (Exception ex)
        {
            if (payment.RefundAttempts >= MaxRetryAttempts)
            {
                payment.Status = PaymentStatus.Failed;
                payment.Notes = AddNote(payment.Notes,
                    $"Partial refund failed after {MaxRetryAttempts} attempts. Last error: {ex.Message}");
    
                _logger.LogError(ex,
                    $"Partial refund FAILED for Payment ID: {payment.Id} after {MaxRetryAttempts} attempts. " +
                    $"Status changed to Failed.");
            }
            else
            {
                payment.ScheduledRefundDate = DateTime.UtcNow.AddHours(1);
                payment.ScheduledTransferDate = DateTime.UtcNow.AddHours(1);
                payment.Notes = AddNote(payment.Notes,
                    $"Partial refund attempt {payment.RefundAttempts} failed: {ex.Message}. Retry scheduled.");
    
                _logger.LogWarning(ex,
                    $"Partial refund attempt {payment.RefundAttempts}/{MaxRetryAttempts} failed for Payment ID: {payment.Id}. " +
                    $"Retry scheduled for {payment.ScheduledRefundDate:yyyy-MM-dd HH:mm:ss}. Error: {ex.Message}");
            }
    
            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private string AddNote(string? existingNotes, string newNote)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var noteEntry = $"[{timestamp}] {newNote}";

        return string.IsNullOrEmpty(existingNotes)
            ? noteEntry
            : $"{existingNotes}\n{noteEntry}";
    }
}