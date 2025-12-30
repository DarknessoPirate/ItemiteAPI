namespace Domain.DTOs.Category;

public class CategoryTreeResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? SvgImage { get; set; }
    public List<CategoryTreeResponse> SubCategories { get; set; } = [];
}