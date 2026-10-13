using CseLibrary.Data.Data;
using CseLibrary.Tests.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Integration;

[Collection(AppCollection.Name)]
public class StartupAndGuardTests(AppFactory app)
{
    [Fact] // step 0.4 app-start check, run against the isolated test DB (DECISION_LOG §33)
    public void App_starts_against_the_test_database_with_all_migrations_applied()
    {
        app.WithService(sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(app.Database.Name, new SqlConnectionStringBuilder(db.Database.GetConnectionString()).InitialCatalog);
            Assert.Equal(db.Database.GetMigrations(), db.Database.GetAppliedMigrations());
            Assert.Contains("20260508164255_InitialCreate", db.Database.GetAppliedMigrations()); // ID survived the 0.3 rename
            Assert.True(db.Users.Any(u => u.Email == AppFactory.AdminEmail));
            return 0;
        });
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

    [Fact] // H1: cleanup targets only the generated name
    public void Guard_accepts_the_generated_name_and_drop_targets_only_it()
    {
        var db = new TestDatabase();
        Assert.StartsWith(TestDatabase.Prefix, db.Name);
        TestDatabase.EnsureSafe(db.ConnectionString);
        db.Drop(); // never created → no-op, must not touch anything else
        Assert.NotEqual(app.Database.Name, db.Name);
    }
}
