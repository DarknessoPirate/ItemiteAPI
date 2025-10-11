namespace Domain.Entities;

public class ListingPhoto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public ListingBase Listing { get; set; }
    public int Order { get; set; } // use this field if user specifies an order of photos to arrange their order
    
    public int PhotoId { get; set; }
    public Photo Photo { get; set; }
}