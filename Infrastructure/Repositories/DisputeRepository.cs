using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DisputeRepository(ItemiteDbContext context) : IDisputeRepository
{
    public async Task AddAsync(Dispute dispute)
    {
        await context.Disputes.AddAsync(dispute);
    }

    public void Update(Dispute dispute)
    {
        context.Disputes.Update(dispute);
    }

    public async Task RemoveAsync(int disputeId)
    {
        var dispute = await context.Disputes.FindAsync(disputeId);
        if (dispute != null)
            context.Disputes.Remove(dispute);
    }

    public async Task<Dispute?> FindByIdAsync(int disputeId)
    {
        return await context.Disputes.FindAsync(disputeId);
    }

    public async Task<Dispute?> FindDetailedByIdAsync(int disputeId)
    {
        return await context.Disputes
        .Include(d => d.Payment)
            .ThenInclude(p => p.Listing)
            .ThenInclude(l => l.ListingPhotos)
            .ThenInclude(lp => lp.Photo)
        .Include(d => d.InitiatedBy)
        .ThenInclude(u => u.ProfilePhoto)
        .Include(d => d.Evidence)
            .ThenInclude(e => e.Photo)
        .FirstOrDefaultAsync(d => d.Id == disputeId);
    }

    public async Task<Dispute?> FindByListingIdAsync(int listingId)
    {
        return await context.Disputes
            .Where(d => d.Payment.ListingId == listingId)
            .FirstOrDefaultAsync();
    }

    public async Task<Dispute?> FindByPaymentIdAsync(int paymentId)
    {
        return await context.Disputes
            .Where(d => d.PaymentId == paymentId)
            .FirstOrDefaultAsync();
    }

    public async Task<Dispute?> FindDetailedByPaymentIdAsync(int paymentId)
    {
        return await context.Disputes
        .Include(d => d.Payment)
            .ThenInclude(p => p.Listing)
            .ThenInclude(l => l.ListingPhotos)
            .ThenInclude(lp => lp.Photo)
        .Include(d => d.InitiatedBy)
        .ThenInclude(u => u.ProfilePhoto)
        .Include(d => d.Evidence)
            .ThenInclude(e => e.Photo)
        .FirstOrDefaultAsync(d => d.PaymentId == paymentId);
    }
}