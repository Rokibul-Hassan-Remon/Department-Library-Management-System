using CseLibrary.Core.Constants;
using CseLibrary.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CseLibrary.Data.Data;

public class SeedAdminOptions
{
    public const string SectionName = "Seed:Admin";
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
        await SeedAdminRoleAsync(db, roleManager, SystemRoles.Admin);
        await SeedAdminUserAsync(userManager, admin);
        await SeedDefaultGeneralConfigsAsync(db);
        await SeedLibraryPoliciesAsync(db);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext db)
    {
        var existing = await db.Permissions.IgnoreQueryFilters().ToListAsync();
        foreach (var (name, module) in Permissions.All())
        {
            var rows = existing.Where(p => p.Name == name).ToList();
            if (rows.Any(p => p.Status != RecordStatus.Deleted)) continue;

            // A built-in permission must exist: restore the latest deleted row, or add it.
            if (rows.MaxBy(p => p.Id) is { } deleted) deleted.Status = RecordStatus.Active;
            else await db.Permissions.AddAsync(new Permission { Name = name, DisplayName = name, Module = module }); // async: HiLo (D-29)
        }
        await db.SaveChangesAsync();
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

        if (!await userManager.IsInRoleAsync(user, SystemRoles.Admin))
        {
            await userManager.AddToRoleAsync(user, SystemRoles.Admin);
        }
    }

    /// <summary>Session 23 defaults: Student 3 books / 14 days, Fellow 5 / 30, one renewal. Only missing types are added.</summary>
    private static async Task SeedLibraryPoliciesAsync(ApplicationDbContext db)
    {
        var existing = await db.LibraryPolicies.Select(p => p.MemberCategory).ToListAsync();
        LibraryPolicy[] defaults =
        [
            new() { MemberCategory = MemberType.Student, MaxActiveLoans = 3, LoanDays = 14, MaxRenewals = 1 },
            new() { MemberCategory = MemberType.Fellow, MaxActiveLoans = 5, LoanDays = 30, MaxRenewals = 1 }
        ];
        foreach (var policy in defaults.Where(p => !existing.Contains(p.MemberCategory)))
            await db.LibraryPolicies.AddAsync(policy); // async: HiLo (D-29)
        await db.SaveChangesAsync();
    }

    private static async Task SeedDefaultGeneralConfigsAsync(ApplicationDbContext db)
    {
        // Adds only missing keys, so an existing database gets new defaults without losing edits.
        GeneralConfig[] defaults =
        [
            new() { Key = "Site.Name", Value = "Default Template", Group = "Site" },
            new() { Key = "Site.SupportEmail", Value = "support@local", Group = "Site" },
            .. LendingConfigKeys.Defaults.Select(d => new GeneralConfig { Key = d.Key, Value = d.Value, Group = "Lending" })
        ];
        var existing = await db.GeneralConfigs.IgnoreQueryFilters().Select(c => c.Key).ToListAsync();
        foreach (var config in defaults.Where(d => !existing.Contains(d.Key)))
            await db.GeneralConfigs.AddAsync(config); // async: HiLo (D-29)
        await db.SaveChangesAsync();
    }
}
