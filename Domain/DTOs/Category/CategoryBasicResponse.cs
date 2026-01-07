namespace Domain.DTOs.Category;

public class CategoryBasicResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PolishName { get; set; }
}