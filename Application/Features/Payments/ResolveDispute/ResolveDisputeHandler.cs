using AutoMapper;
using Domain.DTOs.Payments;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Exceptions;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Payments.ResolveDispute;

// ADMIN ENDPOINT HANDLER TO MAKE A DECISION ABOUT THE DISPUTE( Refund or deny or something)
public class ResolveDisputeHandler(
    UserManager<User> userManager,
    IDisputeRepository disputeRepository,
    IPaymentRepository paymentRepository,
    IStripeConnectService stripeConnectService,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ILogger<ResolveDisputeHandler> logger
) : IRequestHandler<ResolveDisputeCommand, DisputeResponse>
{
    public async Task<DisputeResponse> Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
    {
        var adminUser = await userManager.FindByIdAsync(request.AdminUserId.ToString());
        if (adminUser == null)
            throw new BadRequestException("Invalid user id");

        var dispute = await disputeRepository.FindDetailedByIdAsync(request.DisputeId);
        if (dispute == null)
            throw new NotFoundException("Dispute not found");


        if (dispute.Status == DisputeStatus.Resolved)
            throw new BadRequestException("This dispute has already been resolved");

        var payment = dispute.Payment;

        if (payment.Status != PaymentStatus.Disputed)
            throw new BadRequestException("Payment is not in disputed status");

        if (request.Resolution == DisputeResolution.PartialRefund)
        {
            if (request.PartialRefundAmount > payment.TotalAmount)
                throw new BadRequestException("Partial refund amount cannot exceed the total payment amount");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            switch (request.Resolution)
            {
                case DisputeResolution.RefundBuyer:
                    await HandleFullRefund(payment, dispute, request, adminUser);
                    break;

                case DisputeResolution.Declined:
                    await HandleDeclinedDispute(payment, dispute, request, adminUser);
                    break;

                case DisputeResolution.PartialRefund:
                    await HandlePartialRefund(payment, dispute, request, adminUser);
                    break;

                default:
                    throw new BadRequestException("Invalid resolution type");
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                $"Dispute {dispute.Id} resolved successfully with {request.Resolution} by admin {adminUser.Id}");

            var resolvedDispute = await disputeRepository.FindDetailedByIdAsync(dispute.Id);
            return mapper.Map<DisputeResponse>(resolvedDispute);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, $"Error resolving dispute {request.DisputeId}: {ex.Message}");
            throw;
        }
    }

    private async Task HandleFullRefund(Payment payment, Dispute dispute, ResolveDisputeCommand request, User admin)
    {
        logger.LogInformation($"Processing full refund for Payment ID: {payment.Id}, Amount: {payment.TotalAmount}");

        // Schedule refund to be processed by background service
        payment.RefundAmount = payment.TotalAmount;
        payment.ScheduledRefundDate = DateTime.UtcNow;
        payment.Status = PaymentStatus.RefundScheduled;
        payment.Notes = AddNote(payment.Notes,
            $"Full refund scheduled by admin {admin.Id}. Dispute resolved in favor of buyer.");

        paymentRepository.Update(payment);

        dispute.Status = DisputeStatus.Resolved;
        dispute.Resolution = DisputeResolution.RefundBuyer;
        dispute.ResolvedById = admin.Id;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.RefundAmount = payment.TotalAmount;
        dispute.Notes = request.ReviewerNotes;

        disputeRepository.Update(dispute);
        await unitOfWork.SaveChangesAsync();
    }


    private async Task HandleDeclinedDispute(Payment payment, Dispute dispute, ResolveDisputeCommand request,
        User admin)
    {
        payment.Status = PaymentStatus.Pending; // or create a new status like "TransferScheduled"
        payment.ScheduledTransferDate = DateTime.UtcNow;
        payment.TransferTrigger = TransferTrigger.TimeBased;
        payment.Notes = AddNote(payment.Notes,
            $"Dispute declined by admin {admin.Id}. Transfer to seller scheduled.");

        paymentRepository.Update(payment);

        dispute.Status = DisputeStatus.Resolved;
        dispute.Resolution = DisputeResolution.Declined;
        dispute.ResolvedById = admin.Id;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.Notes = request.ReviewerNotes;

        disputeRepository.Update(dispute);
        await unitOfWork.SaveChangesAsync();
    }

    private async Task HandlePartialRefund(Payment payment, Dispute dispute, ResolveDisputeCommand request, User admin)
    {
        var refundAmount = request.PartialRefundAmount!.Value;
        var sellerAmount = payment.SellerAmount - refundAmount;

        logger.LogInformation(
            $"Processing partial refund for Payment ID: {payment.Id}, Refund: {refundAmount}, Seller gets: {sellerAmount}");

        // Schedule partial refund
        payment.RefundAmount = refundAmount;
        payment.SellerAmount = sellerAmount;
        payment.ScheduledRefundDate = DateTime.UtcNow;
        payment.Status = PaymentStatus.PartialRefundScheduled;
        payment.Notes = AddNote(payment.Notes,
            $"Partial refund of {refundAmount} {payment.Currency.ToUpper()} scheduled by admin {admin.Id}. Seller amount: {sellerAmount}");

        paymentRepository.Update(payment);

        // Update dispute
        dispute.Status = DisputeStatus.Resolved;
        dispute.Resolution = DisputeResolution.PartialRefund;
        dispute.ResolvedById = admin.Id;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.RefundAmount = refundAmount;
        dispute.Notes = request.ReviewerNotes;

        disputeRepository.Update(dispute);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation($"Partial refund scheduled for payment id: {payment.Id}");
    }

    private string AddNote(string? existingNotes, string newNote)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var noteEntry = $"[{timestamp}] {newNote}";

        return string.IsNullOrEmpty(existingNotes)
            ? noteEntry
            : $"{existingNotes}\n{noteEntry}";
    }
}