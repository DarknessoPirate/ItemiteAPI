using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects;

namespace Domain.Entities;

public class ListingBase
{
    public int Id { get; set; }
    [Required]
    [MinLength(5)]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public Location Location { get; set; } = new();
    public int Views { get; set; } = 0;
    public int Followers { get; set; } = 0;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateEnds { get; set; } 
    public bool IsArchived { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    [MaxLength(500)]
    public string? Description { get; set; } = null;
    public int OwnerId { get; set; }
    public required User Owner { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = [];

    public ICollection<ListingPhoto> ListingPhotos { get; set; } = [];
    public ICollection<Message> ListingMessages { get; set; } = [];
    public ICollection<FollowedListing> FollowedListings { get; set; } = [];
}