using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data;
using CseLibrary.Data.Data.Interceptors;
using CseLibrary.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CseLibrary.Data;

public static class DependencyInjection
{
    /// <summary>Needs an <see cref="ICurrentUser"/> registration from the host.</summary>
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditColumnsInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            // Order matters: audit columns and soft delete first, then the audit rows that describe them (D-21).
            .AddInterceptors(sp.GetRequiredService<AuditColumnsInterceptor>(), sp.GetRequiredService<AuditLogInterceptor>()));

        // Password and lockout values come from the "Identity" section of appsettings (D-31).
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                configuration.GetSection("Identity").Bind(options);
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddSignInManager<ActiveUserSignInManager>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
