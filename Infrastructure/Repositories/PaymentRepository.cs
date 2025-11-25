using Domain.Entities;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository(ItemiteDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<Payment?> GetPaymentByStripeChargeIdAsync(string stripeChargeId)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.StripeChargeId == stripeChargeId);
    }

    public async Task<List<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
    {
        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Buyer)
            .Include(p => p.Seller)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPendingPaymentsForTransferAsync()
    {
        var now = DateTime.UtcNow;

        return await context.Payments
            .Include(p => p.Listing)
            .Include(p => p.Seller)
            .Where(p =>
                p.Status == PaymentStatus.Pending &&
                p.TransferTrigger == TransferTrigger.TimeBased &&
                p.ScheduledTransferDate != null &&
                p.ScheduledTransferDate <= now)
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

    public async Task CreatePaymentAsync(Payment payment)
    {
        await context.Payments.AddAsync(payment);
    }

    public void UpdatePayment(Payment payment)
    {
        context.Payments.Update(payment);
    }
}