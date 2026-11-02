using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PawRescue.Application.Interfaces;
using PawRescue.Infrastructure.Data;
using PawRescue.Infrastructure.Messaging;
using PawRescue.Infrastructure.Repositories;
using PawRescue.Infrastructure.Security;
using PawRescue.Infrastructure.Storage;

namespace PawRescue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
        services.Configure<StorageOptions>(config.GetSection(StorageOptions.Section));
        services.Configure<FirebaseOptions>(config.GetSection(FirebaseOptions.Section));
        services.Configure<SeedOptions>(config.GetSection(SeedOptions.Section));

        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Default"),
                npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Unit of Work = the DbContext itself (one per request)
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IRescueAssignmentRepository, RescueAssignmentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Security
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Storage — choose implementation from config (POLYMORPHISM via DI)
        var provider = config[$"{StorageOptions.Section}:Provider"];
        if (string.Equals(provider, "Cloudinary", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IImageStorageService, CloudinaryImageStorageService>();
        else
            services.AddSingleton<IImageStorageService, LocalImageStorageService>();

        // Push notifications
        var firebaseCreds = config[$"{FirebaseOptions.Section}:CredentialsPath"];
        if (!string.IsNullOrWhiteSpace(firebaseCreds) && File.Exists(firebaseCreds))
            services.AddSingleton<IPushNotificationSender, FcmPushNotificationSender>();
        else
            services.AddSingleton<IPushNotificationSender, LoggingPushNotificationSender>();

        services.AddSingleton<IEmailSender, ConsoleEmailSender>();
        services.AddSingleton<IAnimalHealthAnalyzer, NotImplementedHealthAnalyzer>();

        return services;
    }
}
