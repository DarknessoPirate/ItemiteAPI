using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using MediatR;

namespace Application.Features.Payments.GetUserPurchases;

public class GetUserPurchasesQuery : IRequest<PageResponse<PaymentBuyerResponse>>
{
    public int UserId { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}