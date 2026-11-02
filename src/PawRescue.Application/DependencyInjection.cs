using Microsoft.Extensions.DependencyInjection;
using PawRescue.Application.Interfaces;
using PawRescue.Application.Services;

namespace PawRescue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRescueService, RescueService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Swap this line for an AI-based strategy later — nothing else changes.
        services.AddSingleton<IPriorityStrategy, SeverityPriorityStrategy>();

        return services;
    }
}
