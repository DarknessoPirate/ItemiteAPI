
using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IDisputeRepository
{
    Task AddAsync(Dispute dispute);
    void Update(Dispute dispute);
    Task RemoveAsync(int disputeId);
    Task<Dispute?> FindByIdAsync(int disputeId);
    Task<Dispute?> FindDetailedByIdAsync(int disputeId);
    Task<Dispute?> FindByListingIdAsync(int listingId);
    Task<Dispute?> FindByPaymentIdAsync(int paymentId);
    Task<Dispute?> FindDetailedByPaymentIdAsync(int paymentId);

}