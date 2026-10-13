using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using CseLibrary.Data.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

    public TestDatabase Database { get; } = new();
    public SqlCommandCounter Commands { get; } = new();

    public AppFactory()
    {
        // Guard before anything can migrate (startup runs DbSeeder → MigrateAsync).
        TestDatabase.EnsureSafe(Database.ConnectionString);
        // Environment variables override every appsettings file, so the app cannot resolve the dev DB.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", Database.ConnectionString);
        Environment.SetEnvironmentVariable("Seed__Admin__Password", AdminPassword);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", Database.ConnectionString);
        builder.ConfigureTestServices(services =>
            services.ConfigureDbContext<ApplicationDbContext>(o => o.AddInterceptors(Commands)));
    }

    public Task InitializeAsync()
    {
        _ = Services; // starts the host: migrate + seed into the test database
        return Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        Database.Drop();
    }

    public T WithService<T>(Func<IServiceProvider, T> action)
    {
        using var scope = Services.CreateScope();
        return action(scope.ServiceProvider);
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

/// <summary>Counts SQL commands sent by EF Core (N+1 checks, D-43).</summary>
public class SqlCommandCounter : DbCommandInterceptor
{
    private int _count;
    public int Count => Volatile.Read(ref _count);
    public void Reset() => Interlocked.Exchange(ref _count, 0);

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r)
    { Interlocked.Increment(ref _count); return r; }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken ct = default)
    { Interlocked.Increment(ref _count); return ValueTask.FromResult(r); }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand c, CommandEventData e, InterceptionResult<int> r)
    { Interlocked.Increment(ref _count); return r; }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<int> r, CancellationToken ct = default)
    { Interlocked.Increment(ref _count); return ValueTask.FromResult(r); }

    public override InterceptionResult<object> ScalarExecuting(DbCommand c, CommandEventData e, InterceptionResult<object> r)
    { Interlocked.Increment(ref _count); return r; }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<object> r, CancellationToken ct = default)
    { Interlocked.Increment(ref _count); return ValueTask.FromResult(r); }
}
