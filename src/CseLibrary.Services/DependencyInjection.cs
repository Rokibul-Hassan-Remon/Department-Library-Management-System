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
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<ILibraryMemberService, LibraryMemberService>();
        services.AddScoped<ILendingService, LendingService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IEmailSender, LogEmailSender>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();

        return services;
    }
}
