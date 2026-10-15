using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data;
using CseLibrary.Data.Data;
using CseLibrary.Services;
using CseLibrary.Web.Authorization;
using CseLibrary.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        // No mail server yet: every email LogEmailSender "sends" also goes to its own file, kept indefinitely.
        .WriteTo.Logger(emails => emails
            .Filter.ByIncludingOnly(Serilog.Filters.Matching.FromSource<CseLibrary.Services.Services.LogEmailSender>())
            .WriteTo.File("Logs/emails-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: null,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Message:lj}{NewLine}{NewLine}")));

    builder.Services.AddRepositoryLayer(builder.Configuration);
    builder.Services.AddServiceLayer();
    builder.Services.AddHostedService<CseLibrary.Web.Infrastructure.ReminderWorker>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToAccessDenied = AccessDeniedRedirect.HandleAsync; // D-55
    });
    // A changed security stamp (deactivation, deletion) ends existing sessions within this interval (D-31, D-32).
    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        options.ValidationInterval = TimeSpan.FromMinutes(builder.Configuration.GetValue("Security:StampValidationMinutes", 5)));

    // Authenticated by default; public endpoints opt out with [AllowAnonymous] (D-31).
    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await DbSeeder.SeedAsync(db, userManager, roleManager, app.Configuration);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    app.UseStatusCodePagesWithReExecute("/Home/HttpStatus/{0}"); // D-34: friendly page for bodiless 4xx/5xx

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

// Exposes the entry point to WebApplicationFactory<Program> in tests (step 0.5).
public partial class Program;
