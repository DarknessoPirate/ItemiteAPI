using Domain.Entities;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository(ItemiteDbContext context) : IPaymentRepository
{
    private IQueryable<Payment> GetBaseDetailedQuery()
    {
        return context.Payments
            .Include(p => p.Listing)
            .ThenInclude(l => l.ListingPhotos)
            .ThenInclude(lp => lp.Photo)
            .Include(p => p.Buyer)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.Seller)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.ApprovedBy)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.Dispute)
            .ThenInclude(d => d.InitiatedBy)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.Dispute)
            .ThenInclude(d => d.ResolvedBy)
            .ThenInclude(u => u.ProfilePhoto)
            .Include(p => p.Dispute)
            .ThenInclude(d => d.Evidence)
            .ThenInclude(e => e.Photo);
    }

    public async Task<Payment?> FindByIdAsync(int paymentId)
    {
        return await GetBaseDetailedQuery()
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> FindByListingIdAsync(int listingId)
    {
        return await GetBaseDetailedQuery()
            .FirstOrDefaultAsync(p => p.ListingId == listingId);
    }

    public async Task<Payment?> FindByStripeChargeIdAsync(string stripeChargeId)
    {
        return await GetBaseDetailedQuery()
            .FirstOrDefaultAsync(p => p.StripeChargeId == stripeChargeId);
    }

    public async Task<(List<Payment> Payments, int TotalCount)> GetPaymentsByStatusPaginatedAsync(
        PaymentStatus status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = GetBaseDetailedQuery()
            .Where(p => p.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.ChargeDate) // Add ordering
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }


    public async Task<(List<Payment> Payments, int TotalCount)> GetLatestPaymentsPaginatedAsync(int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = GetBaseDetailedQuery();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.ChargeDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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

    public async Task<List<Payment>> FindAllScheduledRefundsAsync()
    {
        var now = DateTime.UtcNow;

        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .Where(p =>
                (p.Status == PaymentStatus.RefundScheduled ||
                 p.Status == PaymentStatus.PartialRefundScheduled) &&
                p.ScheduledRefundDate != null &&
                p.ScheduledRefundDate <= now
            )
            .ToListAsync();
    }

    public async Task<Dictionary<PaymentStatus, int>> GetPaymentCountsByStatusAsync()
    {
        var counts = await context.Payments
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        foreach (PaymentStatus status in Enum.GetValues<PaymentStatus>())
        {
            counts.TryAdd(status, 0);
        }

        return counts;
    }

    public async Task<(List<Payment>, int TotalCount)> GetUserPurchasesPaginatedAsync(int userId, int pageNumber,
        int pageSize)
    {
        var query = GetBaseDetailedQuery()
            .Where(p => p.BuyerId == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.ChargeDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<Payment>, int TotalCount)> GetUserSalesPaginatedAsync(int userId, int pageNumber,
        int pageSize)
    {
        var query = GetBaseDetailedQuery()
            .Where(p => p.SellerId == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.ChargeDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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