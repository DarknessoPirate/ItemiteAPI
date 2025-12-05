using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Payments.GetUserSales;

public class GetUserSalesHandler(
    UserManager<User> userManager,
    IPaymentRepository paymentRepository,
    IMapper mapper
) : IRequestHandler<GetUserSalesQuery, PageResponse<PaymentSellerResponse>>
{
    public async Task<PageResponse<PaymentSellerResponse>> Handle(GetUserSalesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var (payments, totalItems) =
            await paymentRepository.GetUserSalesPaginatedAsync(user.Id, request.PageNumber, request.PageSize);

        var mappedPayments = mapper.Map<List<PaymentSellerResponse>>(payments);

        return new PageResponse<PaymentSellerResponse>(mappedPayments, totalItems, request.PageSize, request.PageNumber);
    }
}