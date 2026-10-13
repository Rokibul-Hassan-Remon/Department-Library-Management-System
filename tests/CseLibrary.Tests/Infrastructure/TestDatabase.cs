using Microsoft.Data.SqlClient;

namespace CseLibrary.Tests.Infrastructure;

/// <summary>
/// Disposable LocalDB database for one test run (D-40). Every operation that can create, migrate
/// or drop a database goes through <see cref="EnsureSafe"/>; cleanup drops only the name generated here.
/// </summary>
public sealed class TestDatabase
{
    public const string Prefix = "CseLibrary_Test_";
    private const string Server = @"(localdb)\MSSQLLocalDB";

    public string Name { get; } = Prefix + Guid.NewGuid().ToString("N");

    public string ConnectionString => Build(Name);

    public static string Build(string databaseName) =>
        $"Server={Server};Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    /// <summary>Throws unless the connection string targets LocalDB and a database named with <see cref="Prefix"/>.</summary>
    public static void EnsureSafe(string connectionString)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
        if (!string.Equals(csb.DataSource, Server, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to use server '{csb.DataSource}': tests run on {Server} only.");
        if (!csb.InitialCatalog.StartsWith(Prefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Refusing to use database '{csb.InitialCatalog}': name must start with '{Prefix}'.");
    }

    public void Drop()
    {
        EnsureSafe(ConnectionString);
        SqlConnection.ClearAllPools();
        using var conn = new SqlConnection(Build("master"));
        conn.Open();
        using var cmd = conn.CreateCommand();
        // Name is generated above (prefix + hex GUID) and re-validated; QUOTENAME guards the identifier anyway.
        cmd.CommandText = """
            IF DB_ID(@name) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@name) + N';';
                EXEC (@sql);
            END
            """;
        cmd.Parameters.AddWithValue("@name", Name);
        cmd.ExecuteNonQuery();
    }
}
