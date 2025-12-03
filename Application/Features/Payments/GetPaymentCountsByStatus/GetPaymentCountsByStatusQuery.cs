using Domain.DTOs.Payments;
using MediatR;

namespace Application.Features.Payments.GetPaymentCountsByStatus;

public class GetPaymentCountsByStatusQuery : IRequest<PaymentStatusCountsResponse>
{
    public int UserId { get; set; }    
}