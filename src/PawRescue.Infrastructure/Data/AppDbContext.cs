using Microsoft.EntityFrameworkCore;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Entities;

namespace PawRescue.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AnimalReport> AnimalReports => Set<AnimalReport>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ReportImage> Images => Set<ReportImage>();
    public DbSet<RescueAssignment> RescueAssignments => Set<RescueAssignment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in Configurations/EntityConfigurations.cs
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
