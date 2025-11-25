using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<Payment?> GetPaymentByStripeChargeIdAsync(string stripeChargeId);
        Task<List<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<List<Payment>> GetPendingPaymentsForTransferAsync(); 
        Task<List<Payment>> GetUserPurchasesAsync(int userId);
        Task<List<Payment>> GetUserSalesAsync(int userId);
        Task CreatePaymentAsync(Payment payment);
        void UpdatePayment(Payment payment);
}