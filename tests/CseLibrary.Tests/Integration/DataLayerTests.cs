using System.Net;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Exceptions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;
using CseLibrary.Data.Data;
using CseLibrary.Data.Repositories;
using CseLibrary.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

/// <summary>Step 0.8: repositories, Unit of Work, record status and audit columns (tests D, E2, F, G1).</summary>
[Collection(AppCollection.Name)]
public class DataLayerTests(AppFactory app)
{
    private const string UserPassword = "Passw0rd!";

    [Fact] // D1 + D4
    public async Task Users_page_costs_the_same_queries_for_any_user_count_and_never_selects_secrets()
    {
        var before = await CountUsersPageCommands();
        await AddRawUsers(45);
        var after = await CountUsersPageCommands();

        Assert.Equal(before, after);
        Assert.DoesNotContain(app.Commands.Texts, t => t.Contains("PasswordHash") || t.Contains("SecurityStamp"));
    }

    [Fact] // D1: roles per user and page boundaries
    public async Task Users_page_returns_roles_and_respects_page_boundaries()
    {
        var (email, roleName) = await CreateUserWithRole();
        await AddRawUsers(3);

        await app.WithServiceAsync(async sp =>
        {
            var users = sp.GetRequiredService<IUserService>();
            var all = await users.GetPageAsync(new PageRequest(1, PageRequest.MaxPageSize));
            Assert.Equal([roleName], all.Items.Single(u => u.Email == email).Roles);

            var first = await users.GetPageAsync(new PageRequest(1, 2));
            Assert.Equal(2, first.Items.Count);
            Assert.Equal(all.TotalCount, first.TotalCount);

            var last = await users.GetPageAsync(new PageRequest(first.TotalPages, 2));
            Assert.Equal(all.TotalCount - (first.TotalPages - 1) * 2, last.Items.Count);
            Assert.Empty((await users.GetPageAsync(new PageRequest(first.TotalPages + 1, 2))).Items);

            var ids = new List<string>();
            for (var p = 1; p <= first.TotalPages; p++)
                ids.AddRange((await users.GetPageAsync(new PageRequest(p, 2))).Items.Select(u => u.Id));
            Assert.Equal(all.TotalCount, ids.Distinct().Count()); // no row skipped or repeated
        });
    }

    [Fact] // D3
    public async Task Permission_lookup_for_a_user_is_one_query() =>
        await app.WithServiceAsync(async sp =>
        {
            var adminId = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(AppFactory.AdminEmail))!.Id;
            app.Commands.Reset();
            var names = await sp.GetRequiredService<IPermissionService>().GetPermissionsForUserAsync(adminId);
            Assert.Equal(1, app.Commands.Count);
            Assert.Contains(CseLibrary.Core.Constants.Permissions.Users.View, names);
        });

    [Fact] // E2
    public Task Unit_of_work_and_identity_stores_share_one_context() =>
        app.WithServiceAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            Assert.Same(db, StoreContext(sp.GetRequiredService<IUserStore<ApplicationUser>>()));
            Assert.Same(db, StoreContext(sp.GetRequiredService<IRoleStore<ApplicationRole>>()));

            var config = new GeneralConfig { Key = NewKey() };
            await sp.GetRequiredService<IUnitOfWork>().GeneralConfigs.AddAsync(config);
            Assert.Equal(EntityState.Added, db.Entry(config).State); // the UoW's repositories use the same instance
        });

    [Fact] // F1: an exception after Identity has saved leaves nothing behind
    public async Task User_create_and_update_roll_back_when_a_later_step_throws()
    {
        var badRole = "NoSuchRole_" + Guid.NewGuid().ToString("N")[..8];
        var email = $"f1_{Guid.NewGuid():N}@test.local";

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => app.WithServiceAsync(sp =>
            sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
            {
                UserName = email, Email = email, Password = UserPassword, Roles = [badRole]
            })));
        Assert.Null(await FindUserAsync(email));

        var (existing, _) = await CreateUserWithRole();
        var id = (await FindUserAsync(existing))!.Id;
        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => app.WithServiceAsync(sp =>
            sp.GetRequiredService<IUserService>().UpdateAsync(new UpdateUserDto
            {
                Id = id, Email = "changed_" + existing, FullName = "Changed", IsActive = true, Roles = [badRole]
            })));
        var reloaded = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().GetByIdAsync(id));
        Assert.Equal(existing, reloaded!.Email);
        Assert.Null(reloaded.FullName);
    }

    [Fact] // F2: a transient failure re-runs the delegate from fresh state and commits once
    public async Task Transient_failure_reruns_the_work_and_commits_once()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(app.Database.ConnectionString, sql => sql.ExecutionStrategy(d => new RetryOnSimulatedFailure(d)))
            .Options;
        await using var db = new ApplicationDbContext(options);
        var uow = new UnitOfWork(db);
        var key = NewKey();
        var attempts = 0;

        var result = await uow.ExecuteInTransactionAsync(async ct =>
        {
            attempts++;
            Assert.Empty(db.ChangeTracker.Entries()); // fresh state on every attempt
            Assert.False(await uow.GeneralConfigs.KeyExistsAsync(key, null, ct)); // first attempt rolled back
            await uow.GeneralConfigs.AddAsync(new GeneralConfig { Key = key }, ct);
            await uow.SaveChangesAsync(ct);
            if (attempts == 1) throw new SimulatedTransientException();
            return Result.Success();
        });

        Assert.True(result.Succeeded);
        Assert.Equal(2, attempts);
        Assert.Equal(1, await db.GeneralConfigs.CountAsync(c => c.Key == key));
    }

    [Fact] // F3
    public Task Failed_result_rolls_back_and_clears_the_change_tracker() =>
        app.WithServiceAsync(async sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            var key = NewKey();

            var result = await uow.ExecuteInTransactionAsync(async ct =>
            {
                await uow.GeneralConfigs.AddAsync(new GeneralConfig { Key = key }, ct);
                await uow.SaveChangesAsync(ct);
                await uow.GeneralConfigs.AddAsync(new GeneralConfig { Key = key + ".2" }, ct); // unsaved
                return Result.Failure(ErrorCodes.Duplicate, "no");
            });

            Assert.False(result.Succeeded);
            Assert.Empty(sp.GetRequiredService<ApplicationDbContext>().ChangeTracker.Entries());
            Assert.Equal(0, await uow.SaveChangesAsync());
            Assert.False(await uow.GeneralConfigs.KeyExistsAsync(key, null));
            Assert.False(await uow.GeneralConfigs.KeyExistsAsync(key + ".2", null));
        });

    [Fact] // F4
    public Task Nested_transaction_is_rejected() =>
        app.WithServiceAsync(async sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                uow.ExecuteInTransactionAsync(_ => uow.ExecuteInTransactionAsync(_ => Task.FromResult(Result.Success()))));
        });

    [Fact] // G1: duplicate key
    public Task Duplicate_key_is_translated() =>
        app.WithServiceAsync(async sp =>
        {
            var uow = sp.GetRequiredService<IUnitOfWork>();
            var key = NewKey();
            await uow.GeneralConfigs.AddAsync(new GeneralConfig { Key = key });
            await uow.GeneralConfigs.AddAsync(new GeneralConfig { Key = key });
            await Assert.ThrowsAsync<DuplicateKeyException>(() => uow.SaveChangesAsync());
        });

    [Fact] // G1: concurrency conflict (ApplicationUser.ConcurrencyStamp is a concurrency token)
    public async Task Concurrency_conflict_is_translated()
    {
        var (email, _) = await CreateUserWithRole();
        await app.WithServiceAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.SingleAsync(u => u.Email == email);

            await app.WithServiceAsync(async other =>
            {
                var otherDb = other.GetRequiredService<ApplicationDbContext>();
                var copy = await otherDb.Users.SingleAsync(u => u.Email == email);
                copy.ConcurrencyStamp = Guid.NewGuid().ToString();
                await otherDb.SaveChangesAsync();
            });

            user.FullName = "Stale write";
            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => sp.GetRequiredService<IUnitOfWork>().SaveChangesAsync());
        });
    }

    [Fact] // D-50 + D-51 through the real request pipeline
    public async Task Delete_is_soft_sets_audit_columns_and_frees_the_key()
    {
        var client = await app.LoggedInClient(AppFactory.AdminEmail, AppFactory.AdminPassword);
        var key = NewKey();
        await AppFactory.PostFormAsync(client, "/GeneralConfig/Create", "/GeneralConfig/Create", new()
        {
            ["Key"] = key, ["Value"] = "v", ["Group"] = "Test", ["Description"] = ""
        });
        var id = (await app.WithServiceAsync(sp => sp.GetRequiredService<IGeneralConfigService>().GetByKeyAsync(key)))!.Id;
        var deleted = await AppFactory.PostFormAsync(client, $"/GeneralConfig/Delete/{id}", $"/GeneralConfig/Delete/{id}", new());
        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);

        await app.WithServiceAsync(async sp =>
        {
            var adminId = (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(AppFactory.AdminEmail))!.Id;
            var row = await sp.GetRequiredService<ApplicationDbContext>().GeneralConfigs
                .IgnoreQueryFilters().AsNoTracking().SingleAsync(c => c.Id == id);
            Assert.Equal(RecordStatus.Deleted, row.Status);
            Assert.Equal(adminId, row.CreatedBy);
            Assert.Equal(adminId, row.UpdatedBy);
            Assert.True(row.CreatedAt > DateTime.UtcNow.AddMinutes(-5));
            Assert.True(row.UpdatedAt >= row.CreatedAt);

            var again = await sp.GetRequiredService<IGeneralConfigService>().CreateAsync(new CreateGeneralConfigDto { Key = key });
            Assert.True(again.Succeeded, again.Error); // filtered unique index ignores the deleted row
        });
    }

    [Fact] // D-51: only Active users can log in
    public async Task Inactive_user_cannot_log_in()
    {
        var (email, roleName) = await CreateUserWithRole();
        var id = (await FindUserAsync(email))!.Id;
        var update = await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().UpdateAsync(new UpdateUserDto
        {
            Id = id, Email = email, IsActive = false, Roles = [roleName]
        }));
        Assert.True(update.Succeeded, update.Error);

        var response = await AppFactory.PostFormAsync(app.NewClient(), "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = email, ["Password"] = UserPassword, ["RememberMe"] = "false"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid login attempt.", await response.Content.ReadAsStringAsync());
    }

    private async Task<int> CountUsersPageCommands()
    {
        app.Commands.Reset();
        await app.WithServiceAsync(sp => sp.GetRequiredService<IUserService>().GetPageAsync(new PageRequest(1, PageRequest.MaxPageSize)));
        return app.Commands.Count;
    }

    // Plain rows without passwords: only the list query is under test, and hashing 45 passwords is slow.
    private Task AddRawUsers(int count) =>
        app.WithServiceAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            for (var i = 0; i < count; i++)
            {
                var name = $"raw_{Guid.NewGuid():N}@test.local";
                db.Users.Add(new ApplicationUser
                {
                    UserName = name, NormalizedUserName = name.ToUpperInvariant(),
                    Email = name, NormalizedEmail = name.ToUpperInvariant(), SecurityStamp = Guid.NewGuid().ToString()
                });
            }
            await db.SaveChangesAsync();
        });

    private async Task<(string Email, string RoleName)> CreateUserWithRole()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var roleName = "Role_" + suffix;
        var email = $"user_{suffix}@test.local";
        await app.WithServiceAsync(async sp =>
        {
            var role = await sp.GetRequiredService<IRoleService>().CreateAsync(new CreateRoleDto { Name = roleName });
            Assert.True(role.Succeeded, role.Error);
            var user = await sp.GetRequiredService<IUserService>().CreateAsync(new CreateUserDto
            {
                UserName = email, Email = email, Password = UserPassword, Roles = [roleName]
            });
            Assert.True(user.Succeeded, user.Error);
        });
        return (email, roleName);
    }

    private Task<ApplicationUser?> FindUserAsync(string email) =>
        app.WithServiceAsync(sp => sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email));

    // The EF stores' generic arity changes between Identity versions; their public Context property does not.
    private static object? StoreContext(object store) => store.GetType().GetProperty("Context")!.GetValue(store);

    private static string NewKey() => "Test." + Guid.NewGuid().ToString("N")[..12];

    private sealed class SimulatedTransientException : Exception;

    private sealed class RetryOnSimulatedFailure(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, maxRetryCount: 2, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => exception is SimulatedTransientException;
    }
}
