namespace Domain.DTOs.Category;

public class CategoryResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PolishName { get; set; }
    public string? Description { get; set; }
    public string? SvgImage { get; set; }
}