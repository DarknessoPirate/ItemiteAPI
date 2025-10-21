using Domain.ValueObjects;
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
        RuleFor(x => x.Images)
            .NotNull().WithMessage("Images array is null")
            .NotEmpty().WithMessage("Images array is empty")
            .Must(images => images.Count > 0).WithMessage("Images array must contain at least one image");
        RuleFor(x => x.ProductListingDto.ImageOrders)
            .Must(orders => orders.Contains(1)).WithMessage("Image orders must contain 1 for being main image")
            .Must(orders => orders.Count == orders.Distinct().Count()).WithMessage("Image orders must be unique");
        RuleFor(x => x)
            .Must(x => x.Images.Count == x.ProductListingDto.ImageOrders.Count).WithMessage("Image orders count must be equal to number of images");

        RuleFor(x => x.ProductListingDto.Location)
            .Must(LocationIsCompleteOrNull)
            .WithMessage("If location is provided, all fields (Longitude, Latitude, Country, City, PostalCode) must be filled")
            .When(x => x.ProductListingDto.Location != null);

        RuleFor(x => x.ProductListingDto.Location.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180")
            .When(x => x.ProductListingDto.Location != null && HasAnyLocationField(x.ProductListingDto.Location));

        RuleFor(x => x.ProductListingDto.Location.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90")
            .When(x => x.ProductListingDto.Location != null && HasAnyLocationField(x.ProductListingDto.Location));

        RuleFor(x => x.ProductListingDto.Location.Country)
            .NotEmpty().WithMessage("Country is required when location is provided")
            .Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
            .When(x => x.ProductListingDto.Location != null && HasAnyLocationField(x.ProductListingDto.Location));

        RuleFor(x => x.ProductListingDto.Location.City)
            .NotEmpty().WithMessage("City is required when location is provided")
            .Length(2, 100).WithMessage("City must be between 2 and 100 characters")
            .When(x => x.ProductListingDto.Location != null && HasAnyLocationField(x.ProductListingDto.Location));

        RuleFor(x => x.ProductListingDto.Location.PostalCode)
            .NotEmpty().WithMessage("Postal code is required when location is provided")
            .Length(2, 20).WithMessage("Postal code must be between 2 and 20 characters")
            .When(x => x.ProductListingDto.Location != null && HasAnyLocationField(x.ProductListingDto.Location));
       
    }
    
    private bool HasAnyLocationField(Location location)
    {
        return location.Longitude.HasValue 
               || location.Latitude.HasValue 
               || !string.IsNullOrWhiteSpace(location.Country) 
               || !string.IsNullOrWhiteSpace(location.City) 
               || !string.IsNullOrWhiteSpace(location.PostalCode);
    }

    private bool LocationIsCompleteOrNull(Location? location)
    {
        if (location == null) return true;
    
        if (!HasAnyLocationField(location)) return true;
    
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.PostalCode);
    }
}