using Domain.Configs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Application.Features.Payments.PurchaseProduct;

public class PurchaseProductHandler(
    IListingRepository<ProductListing> productListingRepository,
    IPaymentRepository paymentRepository,
    IStripeConnectService stripeConnectService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IOptions<PaymentSettings> paymentSettings,
    ILogger<PurchaseProductCommand> logger
) : IRequestHandler<PurchaseProductCommand, int>
{
    public async Task<int> Handle(PurchaseProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productListingRepository.GetListingByIdAsync(request.ProductListingId);
        if (product == null)
        {
            throw new NotFoundException($"Product with id {request.ProductListingId} not found");
        }

        if (product.IsArchived)
        {
            throw new BadRequestException("This product is no longer available");
        }

        if (product.IsSold)
        {
            throw new BadRequestException("This product has already been sold");
        }

        if (product.OwnerId == request.BuyerId)
        {
            throw new BadRequestException("You cannot purchase your own product");
        }

        if (string.IsNullOrEmpty(product.Owner.StripeConnectAccountId))
        {
            throw new BadRequestException("Seller has not set up their payment account yet");
        }

        var isSellerOnboarded = await stripeConnectService.IsAccountFullyOnboardedAsync(
            product.Owner.StripeConnectAccountId);

        if (!isSellerOnboarded)
        {
            throw new BadRequestException("Seller's payment account is not fully set up yet");
        }

        var platformFeePercentage = paymentSettings.Value.PlatformFeePercentage;
        var platformFeeAmount = product.Price * (platformFeePercentage / 100);
        var sellerAmount = product.Price - platformFeeAmount;

        var chargeMetadata = new Dictionary<string, string>
        {
            { "product_id", product.Id.ToString() },
            { "product_name", product.Name },
            { "seller_id", product.OwnerId.ToString() },
            { "buyer_id", request.BuyerId.ToString() },
            { "platform_fee", platformFeeAmount.ToString("F2") }
        };

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var charge = await stripeConnectService.CreateChargeAsync(
                amount: product.Price,
                currency: "pln",
                paymentMethodId: request.PaymentMethodId,
                description: $"Purchase: {product.Name}",
                metadata: chargeMetadata
            );

            var payment = new Payment
            {
                StripeChargeId = charge.Id,
                TotalAmount = product.Price,
                PlatformFeePercentage = platformFeePercentage,
                PlatformFeeAmount = platformFeeAmount,
                SellerAmount = sellerAmount,
                Currency = "pln",
                ListingId = product.Id,
                BuyerId = request.BuyerId,
                SellerId = product.OwnerId,
                Status = PaymentStatus.Pending,
                TransferTrigger = TransferTrigger.TimeBased,
                ScheduledTransferDate = DateTime.UtcNow.AddDays(paymentSettings.Value.TransferDelayDays),
                ChargeDate = DateTime.UtcNow
            };

            await paymentRepository.CreatePaymentAsync(payment);
            await unitOfWork.SaveChangesAsync(cancellationToken); // save here because it needs to generate id
            
            product.IsSold = true;
            product.PaymentId = payment.Id;
            productListingRepository.UpdateListing(product);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await cacheService.RemoveAsync($"{CacheKeys.PRODUCT_LISTING}{product.Id}");
            await cacheService.RemoveByPatternAsync($"{CacheKeys.LISTINGS}*");

            logger.LogInformation(
                $"Payment successful - Product: {product.Id}, Buyer: {request.BuyerId}, Amount: {product.Price} PLN");

            return payment.Id;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error processing purchase for product {request.ProductListingId}: {ex.Message}");
            throw; // Re-throw to let global exception handler deal with it
        }
    }
}
