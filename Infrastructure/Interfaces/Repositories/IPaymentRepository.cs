using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> FindByIdAsync(int paymentId);
    Task<Payment?> FindByListingIdAsync(int listingId);
    Task<Payment?> FindByStripeChargeIdAsync(string stripeChargeId);

    Task<(List<Payment> Payments, int TotalCount)> GetPaymentsByStatusPaginatedAsync(
        PaymentStatus status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(List<Payment> Payments, int TotalCount)> GetLatestPaymentsPaginatedAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    Task<List<Payment>> FindAllPendingAsync();

    Task<Dictionary<PaymentStatus, int>> GetPaymentCountsByStatusAsync();
    Task<List<Payment>> GetUserPurchasesAsync(int userId);
    Task<List<Payment>> GetUserSalesAsync(int userId);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
}