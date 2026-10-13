using CseLibrary.Data;
using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

[Collection(AppCollection.Name)]
public class StartupAndGuardTests(AppFactory app)
{
    [Fact] // step 0.4 app-start check, run against the isolated test DB (DECISION_LOG §33)
    public Task App_starts_against_the_test_database_with_all_migrations_applied() =>
        app.WithServiceAsync(async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(app.Database.Name, new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog);
            Assert.Equal(db.Database.GetMigrations(), await db.Database.GetAppliedMigrationsAsync());
            Assert.Contains("20260508164255_InitialCreate", db.Database.GetMigrations()); // ID survived the 0.3 rename
            Assert.True(await db.Users.AnyAsync(u => u.Email == AppFactory.AdminEmail));
        });

    [Fact] // the guard sees the connection string the app's own registration resolves, before any connection opens
    public void Guard_refuses_the_DbContext_when_the_app_resolves_a_non_test_database()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = @"Server=.\SQLEXPRESS;Database=CseLibrary;Trusted_Connection=True"
        }).Build();
        var services = new ServiceCollection().AddLogging().AddRepositoryLayer(config);
        AppFactory.GuardDbContext(services);

        using var scope = services.BuildServiceProvider().CreateScope();
        var ex = Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        Assert.StartsWith("Refusing to use server", ex.Message);
    }

    [Theory] // H1
    [InlineData(@"Server=(localdb)\MSSQLLocalDB;Database=CseLibrary")]
    [InlineData(@"Server=(localdb)\MSSQLLocalDB;Database=DefaultTemplate")]
    [InlineData(@"Server=(localdb)\MSSQLLocalDB;Database=master")]
    [InlineData(@"Server=(localdb)\MSSQLLocalDB;Database=cselibrary_test_lowercase")]
    [InlineData(@"Server=.\SQLEXPRESS;Database=CseLibrary_Test_0123")]
    public void Guard_refuses_any_database_that_is_not_a_LocalDB_test_database(string connectionString)
    {
        Assert.Throws<InvalidOperationException>(() => TestDatabase.EnsureSafe(connectionString));
    }

    [Fact] // H1: cleanup drops its own database and nothing else
    public void Drop_removes_only_its_own_database()
    {
        var db = new TestDatabase();
        TestDatabase.EnsureSafe(db.ConnectionString);
        try
        {
            Execute($"CREATE DATABASE [{db.Name}]"); // name is prefix + hex GUID
            Assert.True(Exists(db.Name));

            db.Drop();

            Assert.False(Exists(db.Name));
            Assert.True(Exists(app.Database.Name));
        }
        finally
        {
            db.Drop(); // no-op unless an assertion above failed
        }
    }

    private static bool Exists(string name) => Execute("SELECT DB_ID(@name)", name) is not DBNull;

    private static object? Execute(string sql, string? name = null)
    {
        using var conn = new SqlConnection(TestDatabase.Build("master"));
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        if (name is not null) cmd.Parameters.AddWithValue("@name", name);
        return cmd.ExecuteScalar();
    }
}
