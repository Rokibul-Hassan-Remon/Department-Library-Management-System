using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Tests.Infrastructure;

[CollectionDefinition(Name)]
public class AppCollection : ICollectionFixture<AppFactory>
{
    public const string Name = "app";
}

/// <summary>
/// One host per test run: the template's Serilog bootstrap logger can be frozen only once per process
/// (D-43 risk), so every test class shares this fixture via <see cref="AppCollection"/>.
/// </summary>
public class AppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@local";
    public const string AdminPassword = "Test-Admin-Pw1"; // test-only, in memory; never a real secret

    private const string ConnectionVar = "ConnectionStrings__DefaultConnection";
    private const string SeedPasswordVar = "Seed__Admin__Password";
    private readonly string? _oldConnection = Environment.GetEnvironmentVariable(ConnectionVar);
    private readonly string? _oldSeedPassword = Environment.GetEnvironmentVariable(SeedPasswordVar);

    public TestDatabase Database { get; } = new();
    public SqlCommandCounter Commands { get; } = new();

    public AppFactory()
    {
        // Environment variables override every appsettings file, so the app cannot resolve the dev DB.
        // Program.cs reads the connection string eagerly at startup, so a UseSetting override is not enough.
        Environment.SetEnvironmentVariable(ConnectionVar, Database.ConnectionString);
        Environment.SetEnvironmentVariable(SeedPasswordVar, AdminPassword);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            GuardDbContext(services, Commands);
            services.AddScoped<ICurrentUser, TestCurrentUser>();
            services.AddControllersWithViews().AddApplicationPart(typeof(FallbackProbeController).Assembly); // A11
        });

    /// <summary>
    /// Runs after the app's UseSqlServer, before startup seeding opens a connection (DbSeeder → MigrateAsync):
    /// checks the connection string the app actually resolved, not the one generated here.
    /// </summary>
    public static void GuardDbContext(IServiceCollection services, params IInterceptor[] interceptors) =>
        services.ConfigureDbContext<ApplicationDbContext>(o =>
        {
            TestDatabase.EnsureSafe(o.Options.Extensions.OfType<RelationalOptionsExtension>().Single().ConnectionString ?? "");
            o.AddInterceptors(interceptors);
        });

    public async Task InitializeAsync()
    {
        _ = Services; // starts the host: migrate + seed into the test database
        TestCurrentUser.SeedAdminId = await WithServiceAsync(async sp =>
            (await sp.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(AdminEmail))!.Id);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
            Database.Drop();
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionVar, _oldConnection);
            Environment.SetEnvironmentVariable(SeedPasswordVar, _oldSeedPassword);
        }
    }

    public async Task<T> WithServiceAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public async Task WithServiceAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    public HttpClient NewClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });

    public async Task<HttpClient> LoggedInClient(string email, string password)
    {
        var client = NewClient();
        var response = await PostFormAsync(client, "/Account/Login", "/Account/Login", new()
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    /// <summary>GETs <paramref name="formPage"/> for its antiforgery token, then POSTs the form.</summary>
    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string formPage, string postUrl, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(formPage);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(token.Success, $"No antiforgery token on {formPage}");
        fields["__RequestVerificationToken"] = token.Groups[1].Value;
        return await client.PostAsync(postUrl, new FormUrlEncodedContent(fields));
    }
}

/// <summary>Counts and keeps the SQL commands sent by EF Core (N+1 and projection checks, D-43).</summary>
public class SqlCommandCounter : DbCommandInterceptor
{
    private int _count;
    public int Count => Volatile.Read(ref _count);
    public IReadOnlyCollection<string> Texts => _texts;
    public void Reset() { Interlocked.Exchange(ref _count, 0); _texts.Clear(); }

    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _texts = new();
    private void Record(DbCommand c) { Interlocked.Increment(ref _count); _texts.Enqueue(c.CommandText); }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r)
    { Record(c); return r; }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken ct = default)
    { Record(c); return ValueTask.FromResult(r); }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand c, CommandEventData e, InterceptionResult<int> r)
    { Record(c); return r; }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<int> r, CancellationToken ct = default)
    { Record(c); return ValueTask.FromResult(r); }

    public override InterceptionResult<object> ScalarExecuting(DbCommand c, CommandEventData e, InterceptionResult<object> r)
    { Record(c); return r; }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<object> r, CancellationToken ct = default)
    { Record(c); return ValueTask.FromResult(r); }
}
