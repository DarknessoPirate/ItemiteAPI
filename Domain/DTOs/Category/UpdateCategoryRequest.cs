namespace Domain.DTOs.Category;

public class UpdateCategoryRequest
{
    public required string Name { get; set; }
    public required string PolishName { get; set; }
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
}