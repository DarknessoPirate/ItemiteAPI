using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Payments.GetAllPayments;

/// <summary>
/// ADMIN handler to get the paginated list of payments (all status types)
/// get the list of all payments with their relevant data (status, listing, users etc.)
/// </summary>
public class GetLatestPaymentsHandler(
    UserManager<User> userManager,
    IPaymentRepository paymentRepository,
    IMapper mapper
) : IRequestHandler<GetLatestPaymentsQuery, PageResponse<PaymentResponse>>
{
    public async Task<PageResponse<PaymentResponse>> Handle(GetLatestPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var (payments, totalCount) =
            await paymentRepository.GetLatestPaymentsPaginatedAsync(query.PageNumber, query.PageSize,
                cancellationToken);

        var mappedPayments = mapper.Map<List<PaymentResponse>>(payments);

        var pageResponse =
            new PageResponse<PaymentResponse>(mappedPayments, totalCount, query.PageSize, query.PageNumber);

        return pageResponse;
    }
}