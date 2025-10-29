namespace Infrastructure.Interfaces.Services;

public interface IDatabaseSeeder
{
    Task SeedAsync();
    Task CreateInitialCategoriesAsync();
    Task ClearCategoriesAsync();
    Task CreateRolesAsync();
    Task ClearRolesAsync();
    Task CreateInitialUsersAsync();
    Task ClearUsersAsync();
    Task CreateAdminUserAsync();
    Task ClearAdminUserAsync();
}
