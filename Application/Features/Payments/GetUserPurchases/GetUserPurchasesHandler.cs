using AutoMapper;
using Domain.DTOs.Pagination;
using Domain.DTOs.Payments;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Payments.GetUserPurchases;
/// <summary>
/// USER handler to get their purchase history as a buyer
/// Returns buyer-focused view with refund and dispute information
/// </summary>
public class GetUserPurchasesHandler(
    UserManager<User> userManager, 
    IPaymentRepository paymentRepository,
    IMapper mapper
    
    ) : IRequestHandler<GetUserPurchasesQuery, PageResponse<PaymentBuyerResponse>>
{
    public async Task<PageResponse<PaymentBuyerResponse>> Handle(GetUserPurchasesQuery request, CancellationToken cancellationToken)
    {
        
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var (payments, totalItems) =
            await paymentRepository.GetUserPurchasesPaginatedAsync(user.Id, request.PageNumber, request.PageSize);

        var mappedPayments = mapper.Map<List<PaymentBuyerResponse>>(payments);

        return new PageResponse<PaymentBuyerResponse>(mappedPayments, totalItems, request.PageSize, request.PageNumber);

    }
}