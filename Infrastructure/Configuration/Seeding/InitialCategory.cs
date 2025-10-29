namespace Infrastructure.Configuration.Seeding;

public class InitialCategory
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<InitialCategory> SubCategories { get; set; } = [];
}