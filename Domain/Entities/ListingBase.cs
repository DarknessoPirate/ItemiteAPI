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
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateEnds { get; set; } 
    public bool IsArchived { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public DateTime? FeaturedAt { get; set; } = null;
    [MaxLength(500)]
    public string? Description { get; set; } = null;
    public int OwnerId { get; set; }
    public required User Owner { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    
    public ICollection<ListingPhoto> ListingPhotos { get; set; } = new List<ListingPhoto>();
    public ICollection<Message> ListingMessages { get; set; } = new List<Message>();
}