using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payments.GetPaymentsByStatus;

public class GetPaymentsByStatusQuery : IRequest<PageResponse<PaymentResponse>>
{
    public int AdminUserId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}