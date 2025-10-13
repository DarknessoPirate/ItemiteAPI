using Domain.DTOs.ProductListing;
using FluentValidation;

namespace Application.Features.ProductListings.UpdateProductListing;

public class UpdateProductListingValidator : AbstractValidator<UpdateProductListingCommand>
{
    public UpdateProductListingValidator()
    {
        RuleFor(x => x.UpdateDto).NotNull().WithMessage("Product listing request is null");
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("Listing id is empty")
            .NotNull().WithMessage("Listing id is null")
            .GreaterThan(0).WithMessage("Listing id must be greater than 0");
        RuleFor(x => x.UpdateDto.Name)
            .NotEmpty().WithMessage("Product name is empty")
            .NotNull().WithMessage("Product name is null")
            .Length(2, 30).WithMessage("Product name must be between 2 and 30 characters");
        RuleFor(x => x.UpdateDto.Description)
            .NotEmpty().WithMessage("Product description is empty")
            .NotNull().WithMessage("Product description is null")
            .Length(2, 500).WithMessage("Product description must be between 2 and 500 characters");
        RuleFor(x => x.UpdateDto.CategoryId)
            .NotEmpty().WithMessage("Category id is empty")
            .NotNull().WithMessage("Category id is null")
            .GreaterThan(0).WithMessage("Category id must be greater than 0");
        RuleFor(x => x.UpdateDto.Price)
            .NotEmpty().WithMessage("Price is empty")
            .NotNull().WithMessage("Price is null")
            .GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.UpdateDto.Location)
            .NotEmpty().WithMessage("Location is empty")
            .NotNull().WithMessage("Location is null");
        RuleFor(x => x.UpdateDto)
            .Must(HaveUniqueImageOrders)
            .WithMessage("Image orders must be unique across all images (existing and new)")
            .When(x => (x.UpdateDto.ExistingPhotoOrders != null && x.UpdateDto.ExistingPhotoOrders.Any()) 
                       || (x.UpdateDto.NewImagesOrder != null && x.UpdateDto.NewImagesOrder.Any()));
        RuleFor(x => x.UpdateDto)
            .Must(HaveMainImage)
            .WithMessage("At least one image must have order = 1 (main image)")
            .When(x => (x.UpdateDto.ExistingPhotoOrders != null && x.UpdateDto.ExistingPhotoOrders.Any()) 
                       || (x.UpdateDto.NewImagesOrder != null && x.UpdateDto.NewImagesOrder.Any()));
        RuleFor(x => x)
            .Must(command => command.NewImages == null 
                             || command.UpdateDto.NewImagesOrder == null 
                             || command.NewImages.Count == command.UpdateDto.NewImagesOrder.Count)
            .WithMessage("NewImagesOrder length must match NewImages length");
    }

    private bool HaveUniqueImageOrders(UpdateProductListingRequest request)
    {
        var allOrders = new List<int>();
        
        if (request.ExistingPhotoOrders != null)
        {
            allOrders.AddRange(request.ExistingPhotoOrders);
        }
        
        if (request.NewImagesOrder != null)
        {
            allOrders.AddRange(request.NewImagesOrder);
        }
    
        return allOrders.Count == allOrders.Distinct().Count();
    }

    private bool HaveMainImage(UpdateProductListingRequest request)
    {
        var allOrders = new List<int>();
    
        if (request.ExistingPhotoOrders != null)
        {
            allOrders.AddRange(request.ExistingPhotoOrders);
        }
    
        if (request.NewImagesOrder != null)
        {
            allOrders.AddRange(request.NewImagesOrder);
        }
    
        return allOrders.Contains(1);
    }
}