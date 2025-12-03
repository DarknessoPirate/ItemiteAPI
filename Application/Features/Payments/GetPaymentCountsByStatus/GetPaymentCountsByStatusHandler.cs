using Domain.DTOs.Payments;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Payments.GetPaymentCountsByStatus;

/// <summary>
/// ADMIN handler to get the payment counts based on the PaymentStatus, in the form PaymentStatus: count
/// </summary>
public class GetPaymentCountsByStatusHandler(
    UserManager<User> userManager,
    IPaymentRepository paymentRepository
    
    ) : IRequestHandler<GetPaymentCountsByStatusQuery, PaymentStatusCountsResponse>
{
    public async Task<PaymentStatusCountsResponse> Handle(GetPaymentCountsByStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var paymentCounts = await paymentRepository.GetPaymentCountsByStatusAsync();

        return new PaymentStatusCountsResponse
        {
            StatusCounts = paymentCounts,
            TotalPayments = paymentCounts.Values.Sum()
        };
    }
}