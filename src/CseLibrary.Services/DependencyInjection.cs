using CseLibrary.Core.Interfaces.Services;
using CseLibrary.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CseLibrary.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IGeneralConfigService, GeneralConfigService>();

        return services;
    }
}
