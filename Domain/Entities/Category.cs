using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    public int Id { get; set; }
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = null;
    public string? ImageUrl { get; set; } = null;

    public int? ParentCategoryId { get; set; } = null;
    public Category? ParentCategory { get; set; } = null;
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    
    public ICollection<ListingBase> Listings { get; set; } = new List<ListingBase>();
}