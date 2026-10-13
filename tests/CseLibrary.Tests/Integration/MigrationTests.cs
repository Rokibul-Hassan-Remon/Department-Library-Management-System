using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CseLibrary.Tests.Integration;

/// <summary>D-51 data conversion on rows that exist before the migration, up and down, in its own test database.</summary>
public class MigrationTests
{
    private const string Before = "20260508164255_InitialCreate";

    [Fact]
    public async Task RecordStatus_migration_converts_existing_rows_both_ways()
    {
        var database = new TestDatabase();
        TestDatabase.EnsureSafe(database.ConnectionString);
        try
        {
            await using var db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(database.ConnectionString).Options);
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync(Before);

            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO GeneralConfigs ([Key], CreatedAt, IsDeleted) VALUES ('live', SYSUTCDATETIME(), 0), ('gone', SYSUTCDATETIME(), 1);
                INSERT INTO Permissions (Name, CreatedAt, IsDeleted) VALUES ('P.Live', SYSUTCDATETIME(), 0), ('P.Gone', SYSUTCDATETIME(), 1);
                INSERT INTO AspNetUsers (Id, CreatedAt, IsActive, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                VALUES ('on', SYSUTCDATETIME(), 1, 0, 0, 0, 0, 0), ('off', SYSUTCDATETIME(), 0, 0, 0, 0, 0, 0);
                """);

            await migrator.MigrateAsync(); // latest
            Assert.Equal(["gone:3", "live:1"], await Pairs(db, "SELECT [Key] + ':' + CAST(Status AS varchar) AS [Value] FROM GeneralConfigs"));
            Assert.Equal(["P.Gone:3", "P.Live:1"], await Pairs(db, "SELECT Name + ':' + CAST(Status AS varchar) AS [Value] FROM Permissions"));
            Assert.Equal(["off:2", "on:1"], await Pairs(db, "SELECT Id + ':' + CAST(Status AS varchar) AS [Value] FROM AspNetUsers"));
            await Assert.ThrowsAnyAsync<Exception>(() => // CHECK rejects an undefined status
                db.Database.ExecuteSqlRawAsync("UPDATE GeneralConfigs SET Status = 0 WHERE [Key] = 'live'"));
            await db.Database.ExecuteSqlRawAsync( // filtered unique index: a deleted key can be reused
                "INSERT INTO GeneralConfigs ([Key], CreatedAt, Status) VALUES ('gone', SYSUTCDATETIME(), 1)");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM GeneralConfigs WHERE [Key] = 'gone' AND Status = 1");

            await migrator.MigrateAsync(Before); // down
            Assert.Equal(["gone:1", "live:0"], await Pairs(db, "SELECT [Key] + ':' + CAST(IsDeleted AS varchar) AS [Value] FROM GeneralConfigs"));
            Assert.Equal(["off:0", "on:1"], await Pairs(db, "SELECT Id + ':' + CAST(IsActive AS varchar) AS [Value] FROM AspNetUsers"));
        }
        finally
        {
            database.Drop();
        }
    }

    private static async Task<List<string>> Pairs(ApplicationDbContext db, string sql) =>
        (await db.Database.SqlQueryRaw<string>(sql).ToListAsync()).Order().ToList();
}
