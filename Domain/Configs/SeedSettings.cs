namespace Domain.Configs;

public class SeedSettings
{
    public bool SeedingEnabled { get; set; } = false;
    public bool ClearCategories { get; set; } = false;
    public bool CreateInitialCategories { get; set; } = false;
    public bool CreateInitialRoles { get; set; } = false;
    public bool ClearRoles { get; set; } = false;
    public bool CreateInitialUsers { get; set; } = false;
    public bool ClearUsers { get; set; } = false;
    public bool CreateAdminUser { get; set; } = false;
    public bool ClearAdminUser { get; set; } = false;
    public AdminUserSettings AdminUser { get; set; } = new();
}