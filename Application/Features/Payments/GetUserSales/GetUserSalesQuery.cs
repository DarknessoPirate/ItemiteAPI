using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using MediatR;

namespace Application.Features.Payments.GetUserSales;

public class GetUserSalesQuery : IRequest<PageResponse<PaymentSellerResponse>>
{
    public int UserId { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}