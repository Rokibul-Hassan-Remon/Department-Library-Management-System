using CseLibrary.Core.Constants;
using CseLibrary.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CseLibrary.Data.Data;

public class SeedAdminOptions
{
    public const string SectionName = "Seed:Admin";
    public string Role { get; set; } = "Admin";
    public string Email { get; set; } = "admin@local";
    public string? Password { get; set; }
    public string? FullName { get; set; } = "System Administrator";
}

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        var admin = new SeedAdminOptions();
        configuration.GetSection(SeedAdminOptions.SectionName).Bind(admin);

        if (string.IsNullOrWhiteSpace(admin.Password))
            throw new InvalidOperationException(
                $"Configuration '{SeedAdminOptions.SectionName}:Password' is required to seed the admin user. " +
                "Set it in appsettings.json (development) or via user-secrets / environment variables (production).");

        await db.Database.MigrateAsync();
        await SeedPermissionsAsync(db);
        await SeedAdminRoleAsync(db, roleManager, admin.Role);
        await SeedAdminUserAsync(userManager, admin);
        await SeedDefaultGeneralConfigsAsync(db);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext db)
    {
        var existing = await db.Permissions.IgnoreQueryFilters().Select(p => p.Name).ToListAsync();
        var toAdd = Permissions.All()
            .Where(p => !existing.Contains(p.Name))
            .Select(p => new Permission
            {
                Name = p.Name,
                DisplayName = p.Name,
                Module = p.Module
            })
            .ToList();
        if (toAdd.Count > 0)
        {
            db.Permissions.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminRoleAsync(ApplicationDbContext db, RoleManager<ApplicationRole> roleManager, string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new ApplicationRole { Name = roleName, Description = "System administrator" };
            await roleManager.CreateAsync(role);
        }

        var allPermissionIds = await db.Permissions.Select(p => p.Id).ToListAsync();
        var assignedIds = await db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
        var missing = allPermissionIds.Except(assignedIds)
            .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
            .ToList();
        if (missing.Count > 0)
        {
            db.RolePermissions.AddRange(missing);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, SeedAdminOptions options)
    {
        var user = await userManager.FindByEmailAsync(options.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                FullName = options.FullName
            };
            var result = await userManager.CreateAsync(user, options.Password!);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create seed admin user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, options.Role))
        {
            await userManager.AddToRoleAsync(user, options.Role);
        }
    }

    private static async Task SeedDefaultGeneralConfigsAsync(ApplicationDbContext db)
    {
        if (await db.GeneralConfigs.AnyAsync()) return;

        db.GeneralConfigs.AddRange(
            new GeneralConfig { Key = "Site.Name", Value = "Default Template", Group = "Site" },
            new GeneralConfig { Key = "Site.SupportEmail", Value = "support@local", Group = "Site" }
        );
        await db.SaveChangesAsync();
    }
}
