namespace Domain.DTOs.Listing;

public class ListingImageResponse
{
    public int ImageId { get; set; }
    public string? ImageUrl { get; set; }
    public int ImageOrder { get; set; }
}