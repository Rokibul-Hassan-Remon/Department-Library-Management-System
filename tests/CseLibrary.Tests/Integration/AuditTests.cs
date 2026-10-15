using System.Net;
using System.Text.Json;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Results;
using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Step 0.11, entity audit (D-21 – D-23, Session 22 package P1–P3): tests C1 – C3.</summary>
[Collection(AppCollection.Name)]
public class AuditTests(AppFactory app)
{
    [Fact] // C1: Created carries the HiLo id, Updated lists only changed audited fields, Deleted is the soft delete
    public async Task Create_update_and_delete_write_entity_audit_rows()
    {
        var key = "Audit." + Guid.NewGuid().ToString("N")[..8];
        var id = await app.WithServiceAsync(async sp =>
        {
            var configs = sp.GetRequiredService<IGeneralConfigService>();
            Assert.True((await configs.CreateAsync(new CreateGeneralConfigDto { Key = key, Value = "v1", Group = "Test" })).Succeeded);
            var created = (await configs.GetByKeyAsync(key))!;
            Assert.True((await configs.UpdateAsync(new UpdateGeneralConfigDto { Id = created.Id, Key = key, Value = "v2", Group = "Test" })).Succeeded);
            Assert.True((await configs.DeleteAsync(created.Id)).Succeeded);
            return created.Id;
        });

        var rows = await Rows(nameof(GeneralConfig), id.ToString());
        Assert.Equal(["Created", "Updated", "Deleted"], rows.Select(r => r.Action));
        Assert.All(rows, r =>
        {
            Assert.Equal(AuditCategory.Entity, r.Category);
            Assert.Equal(TestCurrentUser.SeedAdminId, r.ActorUserId);
            Assert.True(r.OccurredAtUtc > DateTime.UtcNow.AddMinutes(-5));
        });

        var created = Changes(rows[0]);
        Assert.Equal(key, created["Key"].GetProperty("New").GetString());
        Assert.Equal("Active", created["Status"].GetProperty("New").GetString());
        Assert.False(created.ContainsKey("Id") || created.ContainsKey("CreatedAt") || created.ContainsKey("CreatedBy"));

        var updated = Changes(rows[1]);
        Assert.Equal(["Value"], updated.Keys); // UpdatedAt/UpdatedBy changed too, but are not audited
        Assert.Equal(("v1", "v2"), (updated["Value"].GetProperty("Old").GetString(), updated["Value"].GetProperty("New").GetString()));

        var deleted = Changes(rows[2]);
        Assert.Equal("v2", deleted["Value"].GetProperty("Old").GetString()); // snapshot
        Assert.Equal("Active", deleted["Status"].GetProperty("Old").GetString());
    }

    [Fact] // C2: a rolled-back transaction leaves no audit row
    public async Task Rolled_back_change_leaves_no_audit_row()
    {
        var id = 0;
        await app.WithServiceAsync(async sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            var result = await uow.ExecuteInTransactionAsync(async ct =>
            {
                var config = new GeneralConfig { Key = "Audit." + Guid.NewGuid().ToString("N")[..8] };
                await uow.GeneralConfigs.AddAsync(config, ct);
                id = config.Id;
                await uow.SaveChangesAsync(ct);
                return Result.Failure(ErrorCodes.Duplicate, "roll back");
            });
            Assert.False(result.Succeeded);
        });

        Assert.NotEqual(0, id);
        Assert.Empty(await Rows(nameof(GeneralConfig), id.ToString()));
    }

    [Fact] // C3 (D-22): Identity changes and role permissions create no automatic entity audit rows
    public async Task Identity_and_role_permission_changes_write_no_entity_audit_rows()
    {
        var before = await EntityTypes();
        var permissionId = (await app.WithServiceAsync(sp => sp.GetRequiredService<IPermissionService>().GetAllAsync()))[0].Id;
        var roleName = "Role_" + Guid.NewGuid().ToString("N")[..8];
        var email = $"audit_{Guid.NewGuid():N}@test.local";
        await app.WithServiceAsync(async sp =>
        {
            Assert.True((await sp.GetRequiredService<IRoleService>().CreateAsync(new CreateRoleDto { Name = roleName, PermissionIds = [permissionId] })).Succeeded);
            Assert.True((await sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
            {
                UserName = email, Email = email, Password = "Passw0rd!12", Roles = [roleName]
            })).Succeeded);
        });

        Assert.Equal(before, await EntityTypes());
        Assert.All(before, t => Assert.Contains(t, new[] { nameof(Book), nameof(BookCopy), nameof(GeneralConfig), nameof(Permission) }));
    }

    [Fact] // P1: rows written during a request carry its TraceId (the D-34 Reference ID) as CorrelationId
    public async Task Audit_row_written_in_a_request_carries_the_TraceId()
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        var key = "Audit." + Guid.NewGuid().ToString("N")[..8];
        var response = await AppFactory.PostFormAsync(client, "/GeneralConfig/Create", "/GeneralConfig/Create", new()
        {
            ["Key"] = key, ["Value"] = "v", ["Group"] = "Test", ["Description"] = ""
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var id = (await app.WithServiceAsync(sp => sp.GetRequiredService<IGeneralConfigService>().GetByKeyAsync(key)))!.Id;
        Assert.Matches("^[0-9a-f]{32}$", Assert.Single(await Rows(nameof(GeneralConfig), id.ToString())).CorrelationId);
    }

    private Task<List<AuditLog>> Rows(string entityType, string entityId) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<ApplicationDbContext>().AuditLogs.AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId).OrderBy(a => a.Id).ToListAsync());

    private Task<List<string>> EntityTypes() =>
        app.WithServiceAsync(sp => sp.GetRequiredService<ApplicationDbContext>().AuditLogs
            .Where(a => a.Category == AuditCategory.Entity).Select(a => a.EntityType).Distinct().OrderBy(t => t).ToListAsync());

    private static Dictionary<string, JsonElement> Changes(AuditLog row) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(row.Changes!)!;
}
