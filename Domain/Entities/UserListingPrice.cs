namespace Domain.Entities;

public class UserListingPrice
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public int ListingId { get; set; }
    public ProductListing Listing { get; set; }
    
    public decimal Price { get; set; }
}