using PawRescue.Domain.Common;

namespace PawRescue.Domain.Entities;

/// <summary>Append-only activity log used by the Admin "Activity monitoring" screen.</summary>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    public string Action { get; private set; } = string.Empty;       // e.g. "REPORT_CREATED"
    public string EntityName { get; private set; } = string.Empty;   // e.g. "AnimalReport"
    public string? EntityId { get; private set; }
    public string? Details { get; private set; }                     // JSON (jsonb column)
    public string? IpAddress { get; private set; }

    private AuditLog() { }

    public AuditLog(Guid? userId, string action, string entityName, string? entityId, string? details, string? ipAddress)
    {
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Details = details;
        IpAddress = ipAddress;
    }
}
