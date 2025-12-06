using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using MediatR;

namespace Application.Features.Payments.GetLatestPayments;

public class GetLatestPaymentsQuery : IRequest<PageResponse<PaymentResponse>>
{
    public int AdminUserId { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}