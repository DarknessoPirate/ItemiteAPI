namespace Domain.DTOs.Category;

public class CreateCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
}