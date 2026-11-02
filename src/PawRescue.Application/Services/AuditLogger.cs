using System.Text.Json;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Entities;

namespace PawRescue.Application.Services;

public class AuditLogger : IAuditLogger
{
    private readonly IAuditLogRepository _logs;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public AuditLogger(IAuditLogRepository logs, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _logs = logs;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task LogAsync(string action, string entityName, string? entityId, object? details = null, CancellationToken ct = default)
    {
        // For login/register the user isn't authenticated yet; fall back to the entity id.
        Guid? userId = _currentUser.UserId;
        if (userId is null && entityName == nameof(User) && Guid.TryParse(entityId, out var id))
            userId = id;

        var json = details is null ? null : JsonSerializer.Serialize(details);
        await _logs.AddAsync(new AuditLog(userId, action, entityName, entityId, json, _currentUser.IpAddress), ct);
        await _uow.SaveChangesAsync(ct);
    }
}
