namespace PawRescue.Domain.Common;

/// <summary>
/// ABSTRACTION + INHERITANCE:
/// Every persisted entity derives from this class, so identity and
/// timestamps are defined once and reused everywhere.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Call whenever an entity's state changes.</summary>
    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
