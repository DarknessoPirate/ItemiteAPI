using Domain.Entities;
using Domain.Enums;
using Infrastructure.Configuration;
using Infrastructure.Configuration.Seeding;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class DatabaseSeeder(
    RoleManager<IdentityRole<int>> roleManager,
    UserManager<User> userManager,
    IOptions<SeedSettings> seedSettings,
    IWebHostEnvironment environment, // for checking if we are in dev (if in prod, clearing is not allowed)
    ILogger<DatabaseSeeder> logger
) : IDatabaseSeeder
{
    public async Task SeedAsync()
    {
        if (environment.IsProduction())
        {
            logger.LogWarning("DatabaseSeeder running in Production environment");
        }

        if (environment.IsDevelopment())
        {
            logger.LogInformation("DatabaseSeeder running in Development environment");
        }

        var settings = seedSettings.Value;
        // if seeding is disabled just return without doing anything
        if (!settings.SeedingEnabled)
        {
            logger.LogInformation("Database seeding is disabled");
            return;
        }

        // check the sensitive fields for mistakes and errors
        if (!ValidateSeedSettings())
        {
            logger.LogError("Database seeding aborted due to invalid configuration");
            return;
        }

        // Clearing db options
        if (settings.ClearUsers)
        {
            if (environment.IsDevelopment())
            {
                await ClearUsersAsync();
            }
            else
            {
                logger.LogWarning("Skipping ClearUsers - not allowed in Production environment");
            }
        }

        if (settings.ClearAdminUser)
            await ClearAdminUserAsync();

        if (settings.ClearRoles)
        {
            if (environment.IsDevelopment())
            {
                await ClearRolesAsync();
            }
            else
            {
                logger.LogWarning("Skipping ClearRoles - not allowed in Production environment");
            }
        }


        logger.LogInformation("Starting database seeding...");

        if (settings.CreateInitialRoles)
            await CreateRolesAsync();

        if (settings.CreateInitialUsers)
        {
            if (environment.IsDevelopment())
            {
                await CreateInitialUsersAsync();
            }
            else
            {
                logger.LogWarning("Skipping CreateInitialUsers - not allowed in Production environment");
            }
        }

        if (settings.CreateAdminUser)
            await CreateAdminUserAsync();


        logger.LogInformation("Database seeding completed");
    }

    public async Task CreateRolesAsync()
    {
        logger.LogInformation("Creating roles...");

        var allRoles = Enum.GetValues<Roles>();

        foreach (var role in allRoles)
        {
            var roleName = role.ToString();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation($"Role {roleName} already exists, skipping");
                continue;
            }

            var newRole = new IdentityRole<int>(roleName);
            var result = await roleManager.CreateAsync(newRole);

            if (result.Succeeded)
            {
                logger.LogInformation($"Role {roleName} created successfully");
            }
            else
            {
                logger.LogError("Failed to create role '{RoleName}': {Errors}",
                    roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    public async Task ClearRolesAsync()
    {
        logger.LogInformation("Clearing roles...");
        var roles = roleManager.Roles.ToList();

        if (!roles.Any())
        {
            logger.LogInformation("No roles found to clear");
            return;
        }

        logger.LogInformation("Found {Count} roles to clear", roles.Count);

        foreach (var role in roles)
        {
            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                logger.LogInformation("Role '{RoleName}' deleted successfully", role.Name);
            }
            else
            {
                logger.LogError("Failed to delete role '{RoleName}': {Errors}",
                    role.Name,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation("Role clearing completed");
    }

    public async Task CreateInitialUsersAsync()
    {
        logger.LogInformation("Creating initial users...");
        var usersData = LoadInitialUsersData();

        if (usersData?.Users == null || !usersData.Users.Any())
        {
            logger.LogWarning("No initial users configured");
            return;
        }

        logger.LogInformation("Found {Count} users to create", usersData.Users.Count);

        foreach (var userSettings in usersData.Users)
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(userSettings.Email);
            if (existingUser != null)
            {
                logger.LogInformation("User '{Email}' already exists, skipping", userSettings.Email);
                continue;
            }

            // Create new user
            var newUser = new User
            {
                UserName = userSettings.UserName,
                Email = userSettings.Email,
                EmailConfirmed = true,
                Location = userSettings.Location
            };

            var result = await userManager.CreateAsync(newUser, userSettings.Password);

            if (!result.Succeeded)
            {
                logger.LogError("Failed to create user '{Email}': {Errors}",
                    userSettings.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            logger.LogInformation("User '{UserName}' created successfully", userSettings.UserName);

            // Assign roles
            if (userSettings.Roles.Any())
            {
                foreach (var roleName in userSettings.Roles)
                {
                    if (await roleManager.RoleExistsAsync(roleName))
                    {
                        var roleResult = await userManager.AddToRoleAsync(newUser, roleName);

                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation("Role '{RoleName}' assigned to user '{UserName}'",
                                roleName, userSettings.UserName);
                        }
                        else
                        {
                            logger.LogError("Failed to assign role '{RoleName}' to user '{UserName}': {Errors}",
                                roleName, userSettings.UserName,
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger.LogWarning("Role '{RoleName}' does not exist. Skipping for user '{UserName}'",
                            roleName, userSettings.UserName);
                    }
                }
            }
        }

        logger.LogInformation("Initial users creation completed");
    }

    // skips admins
    public async Task ClearUsersAsync()
    {
        logger.LogInformation("Clearing non-admin users...");
        var users = userManager.Users.ToList();

        if (!users.Any())
        {
            logger.LogInformation("No users found to clear");
            return;
        }

        logger.LogInformation("Found {Count} users", users.Count);
        var adminRoleName = Roles.Admin.ToString();
        var deletedCount = 0;
        var skippedCount = 0;

        foreach (var user in users)
        {
            var isAdmin = await userManager.IsInRoleAsync(user, adminRoleName);
            if (isAdmin)
            {
                logger.LogInformation("Skipping admin user '{UserName}'", user.UserName);
                skippedCount++;
                continue;
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                logger.LogInformation("User '{UserName}' deleted successfully", user.UserName);
                deletedCount++;
            }
            else
            {
                logger.LogError("Failed to delete user '{UserName}': {Errors}", user.UserName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation("User clearing completed. Deleted: {Deleted}, Skipped (Admin): {Skipped}",
            deletedCount, skippedCount);
    }

    public async Task CreateAdminUserAsync()
    {
        var adminSettings = seedSettings.Value.AdminUser;

        logger.LogInformation("Creating admin user...");
        var existingAdmin = await userManager.FindByEmailAsync(adminSettings.Email);
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists, skipping");
            return;
        }

        var adminUser = new User
        {
            UserName = adminSettings.UserName,
            Email = adminSettings.Email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(adminUser, adminSettings.Password);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("Admin user '{UserName}' created successfully", adminSettings.UserName);

        var adminRoleName = Roles.Admin.ToString();
        if (await roleManager.RoleExistsAsync(adminRoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin role assigned to user '{UserName}'", adminSettings.UserName);
            }
            else
            {
                logger.LogError("Failed to assign Admin role to user '{UserName}': {Errors}",
                    adminSettings.UserName,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogWarning("Admin role does not exist. User '{UserName}' created without role.",
                adminSettings.UserName);
        }
    }

    public async Task ClearAdminUserAsync()
    {
        var adminSettings = seedSettings.Value.AdminUser;
        logger.LogInformation("Clearing admin user...");
        var adminUser = await userManager.FindByEmailAsync(adminSettings.Email);

        if (adminUser == null)
        {
            logger.LogInformation("Admin user not found, skipping");
            return;
        }

        var result = await userManager.DeleteAsync(adminUser);

        if (result.Succeeded)
        {
            logger.LogInformation("Admin user '{UserName}' deleted successfully", adminUser.UserName);
        }

        else
        {
            logger.LogError("Failed to delete admin user '{UserName}': {Errors}",
                adminUser.UserName,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private InitialUsersData LoadInitialUsersData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "Seeding", "initial-users.json");

        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("initial-users.json file not found at '{Path}'", jsonPath);
            return new InitialUsersData();
        }

        try
        {
            var jsonContent = File.ReadAllText(jsonPath);
            var usersData = System.Text.Json.JsonSerializer.Deserialize<InitialUsersData>(jsonContent);
            return usersData ?? new InitialUsersData();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load initial users from JSON file");
            return new InitialUsersData();
        }
    }

    private bool ValidateSeedSettings()
    {
        var settings = seedSettings.Value;
        var isValid = true;

        if (string.IsNullOrWhiteSpace(settings.AdminUser.Email))
        {
            logger.LogError("Seed configuration error: Admin email is required");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(settings.AdminUser.UserName))
        {
            logger.LogError("Seed configuration error: Admin username is required");
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(settings.AdminUser.Password))
        {
            logger.LogError("Seed configuration error: Admin password is required");
            isValid = false;
        }
        else if (settings.AdminUser.Password.Length < 10)
        {
            logger.LogError(
                "Seed configuration error: Admin password must be at least 10 characters (current length: {Length})",
                settings.AdminUser.Password.Length);
            isValid = false;
        }

        return isValid;
    }
}