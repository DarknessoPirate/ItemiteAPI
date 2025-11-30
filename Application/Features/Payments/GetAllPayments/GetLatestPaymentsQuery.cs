using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using MediatR;

namespace Application.Features.Payments.GetAllPayments;

public class GetLatestPaymentsQuery : IRequest<PageResponse<PaymentResponse>>
{
    public int UserId { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}