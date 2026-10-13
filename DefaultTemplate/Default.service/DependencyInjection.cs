using Default.core.Interfaces.Services;
using Default.service.Mapping;
using Default.service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Default.service;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IGeneralConfigService, GeneralConfigService>();

        return services;
    }
}
