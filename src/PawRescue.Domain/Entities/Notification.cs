using PawRescue.Domain.Common;
using PawRescue.Domain.Enums;

namespace PawRescue.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid? ReportId { get; private set; }
    public AnimalReport? Report { get; private set; }

    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public Notification(Guid userId, Guid? reportId, NotificationType type, string title, string message)
    {
        UserId = userId;
        ReportId = reportId;
        Type = type;
        Title = title;
        Message = message;
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
