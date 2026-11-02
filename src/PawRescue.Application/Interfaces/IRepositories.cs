using PawRescue.Application.DTOs;
using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Interfaces;

/// <summary>
/// REPOSITORY PATTERN + ABSTRACTION:
/// The Application layer only depends on these interfaces. EF Core lives
/// in Infrastructure and can be swapped (or mocked in unit tests).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRoleAsync(Guid id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetVolunteersInBoxAsync(double minLat, double maxLat, double minLon, double maxLon, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> SearchAsync(string? keyword, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(int? roleId = null, CancellationToken ct = default);
}

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
}

public interface IReportRepository : IRepository<AnimalReport>
{
    Task<AnimalReport?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AnimalReport>> GetInBoundingBoxAsync(
        double minLat, double maxLat, double minLon, double maxLon,
        SeverityLevel? minSeverity, ReportStatus? status, bool openOnly, int limit,
        CancellationToken ct = default);

    Task<(IReadOnlyList<AnimalReport> Items, int Total)> SearchAsync(SearchQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<AnimalReport>> GetByReporterAsync(Guid reporterId, CancellationToken ct = default);
    Task<IReadOnlyList<AnimalReport>> GetAvailableForRescueAsync(int limit, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountOpenAsync(SeverityLevel? severity = null, CancellationToken ct = default);
    Task<int> CountByReporterAndStatusAsync(Guid reporterId, ReportStatus? status, CancellationToken ct = default);
    Task<int> CountCreatedSinceAsync(DateTime since, CancellationToken ct = default);
    Task<Dictionary<ReportStatus, int>> CountByStatusAsync(CancellationToken ct = default);
    Task<Dictionary<AnimalType, int>> CountByAnimalTypeAsync(CancellationToken ct = default);
}

public interface IRescueAssignmentRepository : IRepository<RescueAssignment>
{
    Task<RescueAssignment?> GetActiveForReportAsync(Guid reportId, CancellationToken ct = default);
    Task<IReadOnlyList<RescueAssignment>> GetByVolunteerAsync(Guid volunteerId, AssignmentStatus? status, CancellationToken ct = default);
    Task<int> CountByVolunteerAsync(Guid volunteerId, AssignmentStatus status, CancellationToken ct = default);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<(IReadOnlyList<Notification> Items, int Total)> GetForUserAsync(Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<(IReadOnlyList<AuditLog> Items, int Total)> GetRecentAsync(int page, int pageSize, CancellationToken ct = default);
}

/// <summary>Commits all repository changes made during one request in a single transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
