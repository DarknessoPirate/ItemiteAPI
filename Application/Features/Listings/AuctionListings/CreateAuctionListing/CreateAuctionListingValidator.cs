using Domain.ValueObjects;
using FluentValidation;

namespace Application.Features.Listings.AuctionListings.CreateAuctionListing;

public class CreateAuctionListingValidator : AbstractValidator<CreateAuctionListingCommand>
{
    public CreateAuctionListingValidator()
    {
        RuleFor(x => x.AuctionListingDto).NotNull().WithMessage("Product listing request is null");
        RuleFor(x => x.AuctionListingDto.Name)
            .NotEmpty().WithMessage("Auction name is empty")
            .NotNull().WithMessage("Auction name is null")
            .Length(2, 50).WithMessage("Auction name must be between 2 and 50 characters");
        RuleFor(x => x.AuctionListingDto.Description)
            .NotEmpty().WithMessage("Auction description is empty")
            .NotNull().WithMessage("Auction description is null")
            .Length(2, 2500).WithMessage("Auction description must be between 2 and 2500 characters");
        RuleFor(x => x.AuctionListingDto.CategoryId)
            .NotEmpty().WithMessage("Category id is empty")
            .NotNull().WithMessage("Category id is null")
            .GreaterThan(0).WithMessage("Category id must be greater than 0");
        RuleFor(x => x.AuctionListingDto.StartingBid)
            .NotEmpty().WithMessage("Starting bid is empty")
            .NotNull().WithMessage("Starting bid is null")
            .GreaterThan(0).WithMessage("Starting bid must be greater than 0");
        RuleFor(x => x.AuctionListingDto.DateEnds)
            .GreaterThan(DateTime.UtcNow.AddHours(1)).WithMessage("Auction must last at least one hour")
            .LessThan(DateTime.UtcNow.AddDays(30)).WithMessage("Auction can't be longer than 30 days");
        RuleFor(x => x.Images)
            .NotNull().WithMessage("Images array is null")
            .NotEmpty().WithMessage("Images array is empty")
            .Must(images => images.Count > 0).WithMessage("Images array must contain at least one image")
            .Must(images => images.Count <= 6).WithMessage("Listing cannot have more than 6 images");
        RuleFor(x => x.AuctionListingDto.ImageOrders)
            .Must(orders => orders.Contains(1)).WithMessage("Image orders must contain 1 for being main image")
            .Must(orders => orders.Count == orders.Distinct().Count()).WithMessage("Image orders must be unique");
        RuleFor(x => x)
            .Must(x => x.Images.Count == x.AuctionListingDto.ImageOrders.Count).WithMessage("Image orders count must be equal to number of images");

        RuleFor(x => x.AuctionListingDto.Location)
            .Must(LocationIsCompleteOrNull)
            .WithMessage("If location is provided, all fields (Longitude, Latitude, Country, City, PostalCode) must be filled")
            .When(x => x.AuctionListingDto.Location != null);

        RuleFor(x => x.AuctionListingDto.Location.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180")
            .When(x => x.AuctionListingDto.Location != null && HasAnyLocationField(x.AuctionListingDto.Location));

        RuleFor(x => x.AuctionListingDto.Location.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90")
            .When(x => x.AuctionListingDto.Location != null && HasAnyLocationField(x.AuctionListingDto.Location));

        RuleFor(x => x.AuctionListingDto.Location.Country)
            .NotEmpty().WithMessage("Country is required when location is provided")
            .Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
            .When(x => x.AuctionListingDto.Location != null && HasAnyLocationField(x.AuctionListingDto.Location));

        RuleFor(x => x.AuctionListingDto.Location.City)
            .NotEmpty().WithMessage("City is required when location is provided")
            .Length(2, 100).WithMessage("City must be between 2 and 100 characters")
            .When(x => x.AuctionListingDto.Location != null && HasAnyLocationField(x.AuctionListingDto.Location));

        RuleFor(x => x.AuctionListingDto.Location.State)
            .NotEmpty().WithMessage("State is required when location is provided")
            .Length(2, 100).WithMessage("State must be between 2 and 20 characters")
            .When(x => x.AuctionListingDto.Location != null && HasAnyLocationField(x.AuctionListingDto.Location));
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
}