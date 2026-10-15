using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CseLibrary.Tests.Integration;

/// <summary>
/// D-30 baseline, in its own test database: matches the model, migrates up and down, and carries the HiLo
/// sequences (D-29) and the D-51 status rules that earlier migrations used to add.
/// </summary>
public class MigrationTests
{
    [Fact]
    public async Task Baseline_matches_the_model_and_migrates_up_and_down()
    {
        var database = new TestDatabase();
        TestDatabase.EnsureSafe(database.ConnectionString);
        try
        {
            await using var db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(database.ConnectionString).Options);
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.EndsWith("_InitialCreate", db.Database.GetMigrations().First());

            await db.Database.MigrateAsync();
            Assert.Equal(["BookCopies_HiLo:10", "Books_HiLo:10", "GeneralConfigs_HiLo:10", "Loans_HiLo:10", "Members_HiLo:10", "Permissions_HiLo:10"],
                await Values(db, "SELECT name + ':' + CAST(increment AS varchar) AS [Value] FROM sys.sequences"));

            await Assert.ThrowsAnyAsync<Exception>(() => // CHECK rejects an undefined status
                db.Database.ExecuteSqlRawAsync("INSERT INTO GeneralConfigs (Id, [Key], CreatedAt, Status) VALUES (-1, 'k', SYSUTCDATETIME(), 0)"));
            await db.Database.ExecuteSqlRawAsync( // filtered unique index: a deleted key can be reused once
                "INSERT INTO GeneralConfigs (Id, [Key], CreatedAt, Status) VALUES (-2, 'k', SYSUTCDATETIME(), 3), (-3, 'k', SYSUTCDATETIME(), 1)");
            await Assert.ThrowsAnyAsync<Exception>(() =>
                db.Database.ExecuteSqlRawAsync("INSERT INTO GeneralConfigs (Id, [Key], CreatedAt, Status) VALUES (-4, 'k', SYSUTCDATETIME(), 1)"));

            await db.GetService<IMigrator>().MigrateAsync(Migration.InitialDatabase); // down
            Assert.Equal(["__EFMigrationsHistory"], await Values(db, "SELECT name AS [Value] FROM sys.tables"));
            Assert.Empty(await Values(db, "SELECT name AS [Value] FROM sys.sequences"));
        }
        finally
        {
            database.Drop();
        }
    }

    private static async Task<List<string>> Values(ApplicationDbContext db, string sql) =>
        (await db.Database.SqlQueryRaw<string>(sql).ToListAsync()).Order().ToList();
}
