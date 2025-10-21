using Domain.DTOs.ProductListing;
using Domain.ValueObjects;
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
        RuleFor(x => x)
            .Must(HaveUniqueImageOrders)
            .WithMessage("Image orders must be unique across all images (existing and new)")
            .When(x => (x.UpdateDto.ExistingPhotoOrders != null && x.UpdateDto.ExistingPhotoOrders.Any()) 
                       || (x.NewImages != null && x.NewImages.Any()));
        RuleFor(x => x)
            .Must(HaveMainImage)
            .WithMessage("At least one image must have order = 1 (main image)")
            .When(x => (x.UpdateDto.ExistingPhotoOrders != null && x.UpdateDto.ExistingPhotoOrders.Any()) 
                       || (x.NewImages != null && x.NewImages.Any()));
        RuleFor(x => x.UpdateDto)
            .Must(x => x.ExistingPhotoOrders!.Count == x.ExistingPhotoIds!.Count)
            .WithMessage("Existing photo orders and ids must have same size")
            .When(x => x.UpdateDto.ExistingPhotoOrders != null && x.UpdateDto.ExistingPhotoIds != null &&
                        x.UpdateDto.ExistingPhotoOrders.Any() && x.UpdateDto.ExistingPhotoIds.Any());
        
        RuleFor(x => x.UpdateDto.Location)
            .Must(LocationIsCompleteOrNull)
            .WithMessage("If location is provided, all fields (Longitude, Latitude, Country, City, PostalCode) must be filled")
            .When(x => x.UpdateDto.Location != null);

        RuleFor(x => x.UpdateDto.Location.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));

        RuleFor(x => x.UpdateDto.Location.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));

        RuleFor(x => x.UpdateDto.Location.Country)
            .NotEmpty().WithMessage("Country is required when location is provided")
            .Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));

        RuleFor(x => x.UpdateDto.Location.City)
            .NotEmpty().WithMessage("City is required when location is provided")
            .Length(2, 100).WithMessage("City must be between 2 and 100 characters")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));

        RuleFor(x => x.UpdateDto.Location.PostalCode)
            .NotEmpty().WithMessage("Postal code is required when location is provided")
            .Length(2, 20).WithMessage("Postal code must be between 2 and 20 characters")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));
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
    private bool HaveUniqueImageOrders(UpdateProductListingCommand command)
    {
        var allOrders = new List<int>();
        
        if (command.UpdateDto.ExistingPhotoOrders != null)
        {
            allOrders.AddRange(command.UpdateDto.ExistingPhotoOrders);
        }
        
        if (command.NewImages != null)
        {
            allOrders.AddRange(command.NewImages.Select(i => i.Order));
        }
    
        return allOrders.Count == allOrders.Distinct().Count();
    }

    private bool HaveMainImage(UpdateProductListingCommand command)
    {
        var allOrders = new List<int>();
    
        if (command.UpdateDto.ExistingPhotoOrders != null)
        {
            allOrders.AddRange(command.UpdateDto.ExistingPhotoOrders);
        }
    
        if (command.NewImages != null)
        {
            allOrders.AddRange(command.NewImages.Select(i => i.Order));
        }
    
        return allOrders.Contains(1);
    }
}