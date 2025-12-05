using Application.Exceptions;
using AutoMapper;
using Domain.Configs;
using Domain.DTOs.Listing;
using Domain.DTOs.Payments;
using Domain.DTOs.User;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.DisputePurchase;

/// <summary>
/// BUYER'S handler to create a dispute for a payment, this will pause the seller's automatic transfer until
/// the dispute is manually resolved with the appropriate decision
/// </summary>
public class DisputePurchaseHandler(
    UserManager<User> userManager,
    IDisputeRepository disputeRepository,
    IPaymentRepository paymentRepository,
    IMediaService mediaService,
    IPhotoRepository photoRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IOptions<PaymentSettings> paymentSettings,
    ILogger<DisputePurchaseHandler> logger
) : IRequestHandler<DisputePurchaseCommand, DisputeUserResponse>
{
    public async Task<DisputeUserResponse> Handle(DisputePurchaseCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid user id");

        var payment = await paymentRepository.FindByIdAsync(request.PaymentId);
        if (payment == null)
            throw new BadRequestException("Invalid payment id");

        if (payment.BuyerId != user.Id)
            throw new ForbiddenException("Only the buyer can dispute the payment");

        if (payment.Status == PaymentStatus.Refunded)
            throw new BadRequestException("The payment has already been refunded");


        if (payment.Status == PaymentStatus.Disputed)
            throw new BadRequestException("This payment already has an active dispute");


        if (payment.Status != PaymentStatus.Pending)
            throw new BadRequestException("Cannot create a dispute for this payment");

        var daysSincePurchase = (DateTime.UtcNow - payment.ChargeDate).TotalDays;
        var disputeTimeWindowDays = paymentSettings.Value.DisputeTimeWindowInDays;
        if (daysSincePurchase > disputeTimeWindowDays)
            throw new BadRequestException($"Payments must be disputed within {disputeTimeWindowDays} days");


        var savedPhotosPublicIds = new List<string>();
        var evidenceList = new List<DisputeEvidence>();
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (request.EvidencePhotos != null && request.EvidencePhotos.Any())
            {
                foreach (var evidencePhoto in request.EvidencePhotos)
                {
                    var uploadResult = await mediaService.UploadPhotoAsync(evidencePhoto);
                    if (uploadResult.Error != null)
                    {
                        logger.LogError(
                            $"Failed to upload evidence photo while creating dispute for paymentId: {payment.Id} for listingId: {payment.ListingId}");
                        break;
                    }

                    savedPhotosPublicIds.Add(uploadResult.PublicId);

                    var photo = new Photo
                    {
                        Url = uploadResult.SecureUrl.AbsoluteUri,
                        PublicId = uploadResult.PublicId,
                        FileName = evidencePhoto.FileName
                    };

                    await photoRepository.AddPhotoAsync(photo);

                    evidenceList.Add(new DisputeEvidence
                    {
                        Photo = photo,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            var dispute = new Dispute
            {
                PaymentId = payment.Id,
                InitiatedByUserId = user.Id,
                Reason = request.Reason,
                Description = request.Description,
                Status = DisputeStatus.Open,
                CreatedAt = DateTime.UtcNow,
                Evidence = evidenceList
            };

            await disputeRepository.AddAsync(dispute);

            payment.Status = PaymentStatus.Disputed;
            paymentRepository.Update(payment);

            await unitOfWork.CommitTransactionAsync();


            logger.LogInformation(
                $"Dispute created successfully. Dispute ID: {dispute.Id}, Payment ID: {payment.Id}");

            var createdDispute = await disputeRepository.FindDetailedByIdAsync(dispute.Id);
            return mapper.Map<DisputeUserResponse>(createdDispute);
        }
        catch (Exception ex)
        {
            foreach (var publicId in savedPhotosPublicIds)
            {
                await mediaService.DeleteImageAsync(publicId);
            }

            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error creating dispute for Payment ID: {request.PaymentId}");
            throw;
        }
    }
}