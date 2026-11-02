using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Infrastructure.Data.Configurations;

// Enums are stored as integers (fast, and lets us use >= on Severity).
// database/schema.sql documents the value meanings with CHECK constraints.

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(30).IsRequired();
        b.Property(r => r.Description).HasMaxLength(200);
        b.HasIndex(r => r.Name).IsUnique();

        b.HasData(
            new Role { Id = RoleIds.User, Name = RoleNames.User, Description = "Can report animals and track reports" },
            new Role { Id = RoleIds.Volunteer, Name = RoleNames.Volunteer, Description = "Can accept and handle rescue cases" },
            new Role { Id = RoleIds.Administrator, Name = RoleNames.Administrator, Description = "Manages users, reports and the system" });
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(u => u.Id);
        b.Property(u => u.FullName).HasMaxLength(120).IsRequired();
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
        b.Property(u => u.PhoneNumber).HasMaxLength(20);
        b.Property(u => u.ProfileImageUrl).HasMaxLength(500);
        b.Property(u => u.DeviceToken).HasMaxLength(500);
        b.Property(u => u.PasswordResetTokenHash).HasMaxLength(100);

        b.HasIndex(u => u.Email).IsUnique();
        b.HasIndex(u => new { u.RoleId, u.LastLatitude, u.LastLongitude }); // nearby-volunteer lookup

        b.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> b)
    {
        b.ToTable("Locations", t =>
        {
            t.HasCheckConstraint("CK_Locations_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
            t.HasCheckConstraint("CK_Locations_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
        });
        b.HasKey(l => l.Id);
        b.Property(l => l.Address).HasMaxLength(300);
        b.Property(l => l.Barangay).HasMaxLength(100);
        b.Property(l => l.City).HasMaxLength(100);
        b.Property(l => l.Province).HasMaxLength(100);
        b.HasIndex(l => new { l.Latitude, l.Longitude });
    }
}

public class AnimalReportConfiguration : IEntityTypeConfiguration<AnimalReport>
{
    public void Configure(EntityTypeBuilder<AnimalReport> b)
    {
        b.ToTable("AnimalReports", t =>
        {
            t.HasCheckConstraint("CK_AnimalReports_Priority", "\"PriorityScore\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_AnimalReports_Status", "\"Status\" BETWEEN 0 AND 5");
            t.HasCheckConstraint("CK_AnimalReports_Severity", "\"Severity\" BETWEEN 0 AND 3");
        });
        b.HasKey(r => r.Id);
        b.Property(r => r.Title).HasMaxLength(120).IsRequired();
        b.Property(r => r.Description).HasMaxLength(2000);

        b.HasOne(r => r.Reporter).WithMany(u => u.Reports).HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Location).WithMany().HasForeignKey(r => r.LocationId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.Status, r.Severity });
        b.HasIndex(r => r.CreatedAt);
        b.HasIndex(r => r.ReporterId);
        b.HasIndex(r => r.PriorityScore);

        // Soft-deleted reports disappear from every query automatically.
        b.HasQueryFilter(r => !r.IsDeleted);
    }
}

public class ReportImageConfiguration : IEntityTypeConfiguration<ReportImage>
{
    public void Configure(EntityTypeBuilder<ReportImage> b)
    {
        b.ToTable("Images", t => t.HasCheckConstraint("CK_Images_Size", "\"SizeBytes\" > 0"));
        b.HasKey(i => i.Id);
        b.Property(i => i.Url).HasMaxLength(500).IsRequired();
        b.Property(i => i.StorageKey).HasMaxLength(300).IsRequired();
        b.Property(i => i.ContentType).HasMaxLength(50).IsRequired();
        b.HasOne(i => i.Report).WithMany(r => r.Images).HasForeignKey(i => i.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(i => i.ReportId);
    }
}

public class RescueAssignmentConfiguration : IEntityTypeConfiguration<RescueAssignment>
{
    public void Configure(EntityTypeBuilder<RescueAssignment> b)
    {
        b.ToTable("RescueAssignments");
        b.HasKey(a => a.Id);
        b.Property(a => a.Notes).HasMaxLength(4000);

        b.HasOne(a => a.Report).WithMany(r => r.Assignments).HasForeignKey(a => a.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(a => a.Volunteer).WithMany(u => u.Assignments).HasForeignKey(a => a.VolunteerId).OnDelete(DeleteBehavior.Restrict);

        // Database-level guarantee: only ONE active assignment per report (prevents double-accept race).
        b.HasIndex(a => a.ReportId).IsUnique().HasFilter($"\"Status\" = {(int)AssignmentStatus.Active}")
            .HasDatabaseName("UX_RescueAssignments_ActivePerReport");
        b.HasIndex(a => new { a.VolunteerId, a.Status });
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(n => n.Id);
        b.Property(n => n.Title).HasMaxLength(120).IsRequired();
        b.Property(n => n.Message).HasMaxLength(500).IsRequired();
        b.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(n => n.Report).WithMany().HasForeignKey(n => n.ReportId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(l => l.Id);
        b.Property(l => l.Action).HasMaxLength(60).IsRequired();
        b.Property(l => l.EntityName).HasMaxLength(60).IsRequired();
        b.Property(l => l.EntityId).HasMaxLength(60);
        b.Property(l => l.Details).HasColumnType("jsonb");
        b.Property(l => l.IpAddress).HasMaxLength(45);
        b.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(l => l.CreatedAt);
        b.HasIndex(l => new { l.EntityName, l.EntityId });
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.TokenHash).HasMaxLength(100).IsRequired();
        b.Property(t => t.ReplacedByTokenHash).HasMaxLength(100);
        b.Ignore(t => t.IsActive);
        b.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.HasIndex(t => t.UserId);
    }
}
