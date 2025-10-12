using FluentValidation;

namespace Application.Features.ProductListings.CreateProductListing;

public class CreateProductListingValidator : AbstractValidator<CreateProductListingCommand>
{
    public CreateProductListingValidator()
    {
        RuleFor(x => x.ProductListingDto).NotNull().WithMessage("Product listing request is null");
        RuleFor(x => x.ProductListingDto.Name)
            .NotEmpty().WithMessage("Product name is empty")
            .NotNull().WithMessage("Product name is null")
            .Length(2, 30).WithMessage("Product name must be between 2 and 30 characters");
        RuleFor(x => x.ProductListingDto.Description)
            .NotEmpty().WithMessage("Product description is empty")
            .NotNull().WithMessage("Product description is null")
            .Length(2, 500).WithMessage("Product description must be between 2 and 500 characters");
        RuleFor(x => x.ProductListingDto.CategoryId)
            .NotEmpty().WithMessage("Category id is empty")
            .NotNull().WithMessage("Category id is null")
            .GreaterThan(0).WithMessage("Category id must be greater than 0");
        RuleFor(x => x.ProductListingDto.Price)
            .NotEmpty().WithMessage("Price is empty")
            .NotNull().WithMessage("Price is null")
            .GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.ProductListingDto.Location)
            .NotEmpty().WithMessage("Location is empty")
            .NotNull().WithMessage("Location is null");
        RuleFor(x => x.Images)
            .NotNull().WithMessage("Images array is null")
            .NotEmpty().WithMessage("Images array is empty")
            .Must(images => images.Count > 0).WithMessage("Images array must contain at least one image");
        RuleFor(x => x)
            .Must(x => x.Images.Count == x.ProductListingDto.ImageOrders.Count).WithMessage("Image orders count must be equal to number of images");
    }
}