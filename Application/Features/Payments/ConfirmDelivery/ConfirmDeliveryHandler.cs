using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payments.ConfirmDelivery;

/// <summary>
/// BUYER's handler to confirm the product's delivery.
/// This will speed up the seller's payment transfer.
/// </summary>
public class ConfirmDeliveryHandler(
    UserManager<User> userManager,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ConfirmDeliveryCommand>
{
    public async Task Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var payment = await paymentRepository.FindByListingIdAsync(request.ListingId);
        if (payment == null)
            throw new BadRequestException("Invalid payment id");

        if (payment.BuyerId != request.UserId)
            throw new ForbiddenException("Not authorized to confirm delivery");

        if (payment.Status == PaymentStatus.Transferred) // don't need the confirmation if already transferred
            return;

        if (payment.Status != PaymentStatus.Pending)
            throw new BadRequestException("Invalid operation on the payment");

        payment.TransferTrigger = TransferTrigger.DeliveryConfirmed;
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}