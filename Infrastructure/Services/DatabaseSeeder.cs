using Domain.Configs;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Configuration;
using Infrastructure.Configuration.Seeding;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class DatabaseSeeder(
    ICategoryRepository categoryRepository,
    RoleManager<IdentityRole<int>> roleManager,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
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

        if (settings.ClearCategories)
        {
            if (environment.IsDevelopment())
            {
                await ClearCategoriesAsync();
            }
            else
            {
                logger.LogWarning("Skipping ClearCategories - not allowed in Production environment");
            }
        }

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

        if (settings.CreateInitialCategories)
        {
            if (environment.IsProduction())
            {
                logger.LogWarning(
                    "Creating initial categories in PRODUCTION environment - ensure this is intentional!");
            }

            await CreateInitialCategoriesAsync();
        }

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

    public async Task CreateInitialCategoriesAsync()
    {
        logger.LogInformation("Creating initial categories...");
        var categoriesData = LoadInitialCategoriesData();

        if (categoriesData.Categories.Count == 0)
        {
            logger.LogWarning("No initial categories found in config file");
            return;
        }

        // check for duplicates in root categories
        var rootNameGroups = categoriesData.Categories.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var duplicateRoots = rootNameGroups.Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateRoots.Any())
        {
            logger.LogWarning(
                "Duplicate root category names found in JSON: {Names}. Only the first occurrence will be processed.",
                string.Join(", ", duplicateRoots));
        }

        // take only the first occurrence of each root category name
        var uniqueCategories = rootNameGroups.Select(g => g.First()).ToList();

        foreach (var categoryData in uniqueCategories)
        {
            // validate tree before creating
            if (!ValidateCategoryTree(categoryData))
            {
                logger.LogError("Validation failed for category tree starting with '{Name}'. Skipping this tree.",
                    categoryData.Name);
                continue;
            }

            // checks if root category with the name already exists, if it exists, skip the whole tree creation of that category
            if (await categoryRepository.RootCategoryExistsByName(categoryData.Name))
            {
                logger.LogInformation("Root category '{Name}' already exists, skipping entire tree", categoryData.Name);
                continue;
            }

            // recursively create the subcategories if it has any
            await CreateCategoryTreeRecursive(categoryData, null, null);
        }

        logger.LogInformation("Initial categories creation completed");
    }

    public async Task ClearCategoriesAsync()
    {
        logger.LogInformation("Clearing categories...");
        var categories = await categoryRepository.GetAllCategories();

        if (categories.Count == 0)
        {
            logger.LogInformation("No categories found to clear");
            return;
        }

        logger.LogInformation("Found {Count} categories to clear", categories.Count);

        foreach (var category in categories)
        {
            categoryRepository.DeleteCategory(category);
        }

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Categories clearing completed");
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

    private async Task CreateCategoryTreeRecursive(InitialCategory categoryData, int? parentId, int? rootId)
    {
        if (string.IsNullOrWhiteSpace(categoryData.Name))
        {
            logger.LogWarning("Skipping category with empty name");
            return;
        }

        if (categoryData.Name.Length < 2 || categoryData.Name.Length > 50)
        {
            logger.LogError("Category name '{Name}' must be between 2 and 50 characters. Skipping.", categoryData.Name);
            return;
        }

        if (categoryData.Description?.Length > 100)
        {
            logger.LogError("Category '{Name}' description exceeds 100 characters. Skipping.", categoryData.Name);
            return;
        }


        // might happen if partial failure occured when using seeder and json didn't change 
        if (rootId != null)
        {
            if (await categoryRepository.CategoryExistsByNameInTree(categoryData.Name, rootId.Value))
            {
                logger.LogError("Category '{Name}' already exists in this tree. Skipping.", categoryData.Name);
                return;
            }
        }

        var newCategory = new Category
        {
            Name = categoryData.Name,
            Description = categoryData.Description,
            ImageUrl = categoryData.ImageUrl,
            ParentCategoryId = parentId,
            RootCategoryId = rootId
        };

        await categoryRepository.CreateCategory(newCategory);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Category '{Name}' created successfully with ID {Id}", newCategory.Name, newCategory.Id);

        // if this is the first category use its id as root for the children, if rootId variable is already initialized use it instead
        var currentRootId = rootId ?? newCategory.Id;

        if (categoryData.SubCategories.Count > 0)
        {
            logger.LogInformation("Creating {Count} subcategories for '{Name}'", categoryData.SubCategories.Count,
                newCategory.Name);

            foreach (var subCategoryData in categoryData.SubCategories)
            {
                await CreateCategoryTreeRecursive(subCategoryData, newCategory.Id, currentRootId);
            }
        }
    }

    private bool ValidateCategoryTree(InitialCategory categoryData, HashSet<string>? namesInTree = null)
    {
        namesInTree ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (namesInTree.Contains(categoryData.Name))
        {
            logger.LogError(
                "Duplicate category name '{Name}' found within the same tree, skipping the whole tree, fix the json first",
                categoryData.Name);
            return false;
        }

        namesInTree.Add(categoryData.Name);

        if (categoryData.SubCategories.Count > 0)
        {
            foreach (var subCategory in categoryData.SubCategories)
            {
                if (!ValidateCategoryTree(subCategory, namesInTree))
                    return false;
            }
        }

        return true;
    }

    private InitialCategoriesData LoadInitialCategoriesData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "Seeding", "initial-categories.json");

        if (!File.Exists(jsonPath))
        {
            logger.LogWarning(
                "initial-categories.json file not found at '{Path}' See the example file and create the config file properly",
                jsonPath);
            return new InitialCategoriesData();
        }

        try
        {
            var jsonContent = File.ReadAllText(jsonPath);
            var categoriesData = System.Text.Json.JsonSerializer.Deserialize<InitialCategoriesData>(jsonContent);
            return categoriesData ?? new InitialCategoriesData();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load initial categories from JSON file");
            return new InitialCategoriesData();
        }
    }

    private InitialUsersData LoadInitialUsersData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "Seeding", "initial-users.json");

        if (!File.Exists(jsonPath))
        {
            logger.LogWarning(
                "initial-users.json file not found at '{Path}'. See the example file and create the config file properly",
                jsonPath);
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