using MediatR;

namespace Application.Features.Payments.PurchaseProduct;

public class PurchaseProductCommand : IRequest<int>
{
    public int BuyerId { get; set; }
    public int ProductListingId { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
}