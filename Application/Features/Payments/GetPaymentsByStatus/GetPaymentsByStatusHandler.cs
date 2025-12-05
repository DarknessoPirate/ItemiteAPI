using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.GetPaymentsByStatus;

/// <summary>
/// ADMIN handler to fetch the paginated payment list, with the specified PaymentStatus
/// </summary>
public class GetPaymentsByStatusHandler(
    UserManager<User> userManager,
    IPaymentRepository paymentRepository,
    IMapper mapper
) : IRequestHandler<GetPaymentsByStatusQuery, PageResponse<PaymentResponse>>
{
    public async Task<PageResponse<PaymentResponse>> Handle(GetPaymentsByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var adminUser = await userManager.FindByIdAsync(request.AdminUserId.ToString());
        if (adminUser == null)
            throw new BadRequestException("Admin user not found");

        var (payments, totalItems) = await paymentRepository.GetPaymentsByStatusPaginatedAsync(
            request.PaymentStatus,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var mappedPayments = mapper.Map<List<PaymentResponse>>(payments);

        var pageResponse =
            new PageResponse<PaymentResponse>(mappedPayments, totalItems, request.PageSize, request.PageNumber);

        return pageResponse;
    }
}