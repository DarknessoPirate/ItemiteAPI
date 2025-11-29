using Domain.Entities;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository(ItemiteDbContext context) : IPaymentRepository
{
    public async Task<Payment?> FindByIdAsync(int paymentId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> FindByListingIdAsync(int listingId)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.ListingId == listingId);
    }

    public async Task<Payment?> FindByStripeChargeIdAsync(string stripeChargeId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.StripeChargeId == stripeChargeId);
    }

    public async Task<List<Payment>> FindAllByStatusAsync(PaymentStatus status)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<List<Payment>> FindAllPendingAsync()
    {
        var now = DateTime.UtcNow;

        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Seller)
            .Where(p =>
                p.Status == PaymentStatus.Pending &&
                (
                    // Time-based: scheduled date has passed
                    (p.TransferTrigger == TransferTrigger.TimeBased &&
                     p.ScheduledTransferDate != null &&
                     p.ScheduledTransferDate <= now)
                    ||
                    // Delivery confirmed: ready to transfer immediately
                    (p.TransferTrigger == TransferTrigger.DeliveryConfirmed)
                )
            )
            .ToListAsync();
    }

    public async Task<List<Payment>> GetUserPurchasesAsync(int userId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Seller)
            .Where(p => p.BuyerId == userId)
            .OrderByDescending(p => p.ChargeDate)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetUserSalesAsync(int userId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Where(p => p.SellerId == userId)
            .OrderByDescending(p => p.ChargeDate)
            .ToListAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        await context.Payments.AddAsync(payment);
    }

    public void Update(Payment payment)
    {
        context.Payments.Update(payment);
    }
}