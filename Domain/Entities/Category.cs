using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    public int Id { get; set; }
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<ListingBase> Listings { get; set; } = new List<ListingBase>();
}