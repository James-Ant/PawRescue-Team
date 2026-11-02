using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Notifications;

/// <summary>
/// ABSTRACTION + INHERITANCE + POLYMORPHISM in one place.
/// NotificationService.NotifyAsync(template) works with ANY subclass;
/// each subclass decides its own title/message (open for extension,
/// closed for modification — the "O" in SOLID).
/// </summary>
public abstract class NotificationTemplate
{
    protected NotificationTemplate(Guid? reportId) => ReportId = reportId;

    public Guid? ReportId { get; }
    public abstract NotificationType Type { get; }
    public abstract string Title { get; }
    public abstract string BuildMessage();

    public Notification ToEntity(Guid userId) => new(userId, ReportId, Type, Title, BuildMessage());
}

public sealed class RescueAcceptedNotification : NotificationTemplate
{
    private readonly string _reportTitle;
    private readonly string _volunteerName;

    public RescueAcceptedNotification(Guid reportId, string reportTitle, string volunteerName) : base(reportId)
    {
        _reportTitle = reportTitle;
        _volunteerName = volunteerName;
    }

    public override NotificationType Type => NotificationType.RescueAccepted;
    public override string Title => "Rescue accepted 🐾";
    public override string BuildMessage() => $"{_volunteerName} is on the way for \"{_reportTitle}\".";
}

public sealed class StatusChangedNotification : NotificationTemplate
{
    private readonly string _reportTitle;
    private readonly ReportStatus _newStatus;

    public StatusChangedNotification(Guid reportId, string reportTitle, ReportStatus newStatus) : base(reportId)
    {
        _reportTitle = reportTitle;
        _newStatus = newStatus;
    }

    public override NotificationType Type => NotificationType.StatusChanged;
    public override string Title => "Report status updated";

    public override string BuildMessage() => _newStatus switch
    {
        ReportStatus.Rescued => $"Good news! \"{_reportTitle}\" has been rescued. Thank you for reporting!",
        ReportStatus.InProgress => $"Rescue for \"{_reportTitle}\" is now in progress.",
        _ => $"\"{_reportTitle}\" is now {Humanize(_newStatus)}."
    };

    private static string Humanize(ReportStatus s) => s switch
    {
        ReportStatus.UnderReview => "under review",
        ReportStatus.RescueAssigned => "assigned to a rescuer",
        _ => s.ToString().ToLowerInvariant()
    };
}

public sealed class NewNearbyReportNotification : NotificationTemplate
{
    private readonly string _reportTitle;
    private readonly SeverityLevel _severity;
    private readonly double _distanceKm;

    public NewNearbyReportNotification(Guid reportId, string reportTitle, SeverityLevel severity, double distanceKm) : base(reportId)
    {
        _reportTitle = reportTitle;
        _severity = severity;
        _distanceKm = distanceKm;
    }

    public override NotificationType Type => NotificationType.NewNearbyReport;
    public override string Title => _severity >= SeverityLevel.High ? "🚨 Urgent animal nearby" : "Animal in need nearby";
    public override string BuildMessage() => $"\"{_reportTitle}\" ({_severity}) was reported {_distanceKm:0.0} km from you.";
}
