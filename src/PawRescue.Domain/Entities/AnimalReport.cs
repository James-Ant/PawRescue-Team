using PawRescue.Domain.Common;
using PawRescue.Domain.Enums;

namespace PawRescue.Domain.Entities;

/// <summary>
/// The core aggregate. ENCAPSULATION is strongest here: the status can
/// ONLY move along the allowed workflow, enforced by ChangeStatus().
/// </summary>
public class AnimalReport : BaseEntity
{
    // Allowed workflow: Reported → UnderReview → RescueAssigned → InProgress → Rescued → Closed
    // (Closed is also reachable from any state, e.g. duplicate / false report.)
    private static readonly Dictionary<ReportStatus, ReportStatus[]> AllowedTransitions = new()
    {
        [ReportStatus.Reported]       = new[] { ReportStatus.UnderReview, ReportStatus.RescueAssigned, ReportStatus.Closed },
        [ReportStatus.UnderReview]    = new[] { ReportStatus.RescueAssigned, ReportStatus.Closed },
        [ReportStatus.RescueAssigned] = new[] { ReportStatus.InProgress, ReportStatus.UnderReview, ReportStatus.Closed },
        [ReportStatus.InProgress]     = new[] { ReportStatus.Rescued, ReportStatus.Closed },
        [ReportStatus.Rescued]        = new[] { ReportStatus.Closed },
        [ReportStatus.Closed]         = Array.Empty<ReportStatus>()
    };

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AnimalType AnimalType { get; private set; }
    public AnimalCondition Condition { get; private set; }
    public SeverityLevel Severity { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Reported;

    /// <summary>0–100. Computed by an IPriorityStrategy (see Application layer).</summary>
    public int PriorityScore { get; private set; }

    public Guid ReporterId { get; private set; }
    public User Reporter { get; private set; } = null!;

    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    public ICollection<ReportImage> Images { get; private set; } = new List<ReportImage>();
    public ICollection<RescueAssignment> Assignments { get; private set; } = new List<RescueAssignment>();

    private AnimalReport() { }

    public AnimalReport(Guid reporterId, string title, string description, AnimalType animalType,
                        AnimalCondition condition, SeverityLevel severity, Location location)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (description?.Length > 2000) throw new DomainException("Description is too long (max 2000).");

        ReporterId = reporterId;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        AnimalType = animalType;
        Condition = condition;
        Severity = severity;
        Location = location;
        LocationId = location.Id;
    }

    public bool IsOpen => Status is not (ReportStatus.Rescued or ReportStatus.Closed);

    public void UpdateDetails(string title, string description, AnimalType type, AnimalCondition condition, SeverityLevel severity)
    {
        if (!IsOpen) throw new DomainException("A rescued or closed report can no longer be edited.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        AnimalType = type;
        Condition = condition;
        Severity = severity;
        Touch();
    }

    public bool CanTransitionTo(ReportStatus next) => AllowedTransitions[Status].Contains(next);

    public void ChangeStatus(ReportStatus next)
    {
        if (next == Status) return;
        if (!CanTransitionTo(next))
            throw new DomainException($"Cannot change status from {Status} to {next}.");

        Status = next;
        Touch();
    }

    public void SetPriority(int score)
    {
        PriorityScore = Math.Clamp(score, 0, 100);
        Touch();
    }

    public void AddImage(ReportImage image)
    {
        if (Images.Count >= 5) throw new DomainException("A report can have at most 5 images.");
        if (!Images.Any()) image.MarkAsPrimary(true);
        Images.Add(image);
        Touch();
    }

    /// <summary>Soft delete keeps history for audit/statistics.</summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }
}
