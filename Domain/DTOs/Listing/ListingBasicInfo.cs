namespace Domain.DTOs.Listing;

public class ListingBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MainImageUrl { get; set; }
    public int OwnerId { get; set; }
    public string ListingType { get; set; }
    public string Price { get; set; }
}