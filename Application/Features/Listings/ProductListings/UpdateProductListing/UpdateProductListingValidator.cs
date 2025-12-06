using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Listings.ProductListings.UpdateProductListing;

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
       
        RuleFor(x => x.UpdateDto.ExistingPhotoIds)
            .NotNull().WithMessage("ExistingPhotoIds cannot be null");

        RuleFor(x => x.UpdateDto.ExistingPhotoOrders)
            .NotNull().WithMessage("ExistingPhotoOrders cannot be null");

        RuleFor(x => x.NewImages)
            .NotNull().WithMessage("NewImages cannot be null");

        RuleFor(x => x)
            .Must(x => x.UpdateDto.ExistingPhotoOrders.Count == x.UpdateDto.ExistingPhotoIds.Count)
            .WithMessage("ExistingPhotoOrders count must match ExistingPhotoIds count");

        RuleFor(x => x)
            .Must(HaveUniqueImageOrders)
            .WithMessage("Image orders must be unique across existing and new images");

        RuleFor(x => x)
            .Must(HaveMainImage)
            .WithMessage("At least one image must have order = 1");
        
        
        RuleFor(x => x.UpdateDto.Location)
            .NotEmpty().WithMessage("Location is empty")
            .NotNull().WithMessage("Location is null");
        
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

        RuleFor(x => x.UpdateDto.Location.State)
            .NotEmpty().WithMessage("State is required when location is provided")
            .Length(2, 100).WithMessage("State must be between 2 and 100 characters")
            .When(x => x.UpdateDto.Location != null && HasAnyLocationField(x.UpdateDto.Location));
    }

    private bool HasAnyLocationField(Location location)
    {
        return location.Longitude.HasValue 
               || location.Latitude.HasValue 
               || !string.IsNullOrWhiteSpace(location.Country) 
               || !string.IsNullOrWhiteSpace(location.City) 
               || !string.IsNullOrWhiteSpace(location.State);
    }

    private bool LocationIsCompleteOrNull(Location? location)
    {
        if (location == null) return true;
        
        if (!HasAnyLocationField(location)) return true;
        
        return location.Longitude.HasValue 
               && location.Latitude.HasValue 
               && !string.IsNullOrWhiteSpace(location.Country) 
               && !string.IsNullOrWhiteSpace(location.City) 
               && !string.IsNullOrWhiteSpace(location.State);
    }
    private bool HaveUniqueImageOrders(UpdateProductListingCommand command)
    {
        var orders = new List<int>();

        orders.AddRange(command.UpdateDto.ExistingPhotoOrders);
        orders.AddRange(command.NewImages.Select(x => x.Order));

        return orders.Count == orders.Distinct().Count();
    }

    private bool HaveMainImage(UpdateProductListingCommand command)
    {
        return command.UpdateDto.ExistingPhotoOrders.Contains(1)
               || command.NewImages.Any(i => i.Order == 1);
    }
}