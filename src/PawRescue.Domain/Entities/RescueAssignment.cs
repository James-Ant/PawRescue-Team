using PawRescue.Domain.Common;
using PawRescue.Domain.Enums;

namespace PawRescue.Domain.Entities;

public class RescueAssignment : BaseEntity
{
    public Guid ReportId { get; private set; }
    public AnimalReport Report { get; private set; } = null!;

    public Guid VolunteerId { get; private set; }
    public User Volunteer { get; private set; } = null!;

    public AssignmentStatus Status { get; private set; } = AssignmentStatus.Active;
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    private RescueAssignment() { }

    public RescueAssignment(Guid reportId, Guid volunteerId)
    {
        ReportId = reportId;
        VolunteerId = volunteerId;
    }

    public void AddNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return;
        Notes = string.IsNullOrEmpty(Notes)
            ? $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note.Trim()}"
            : $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note.Trim()}";
        Touch();
    }

    public void Complete()
    {
        if (Status != AssignmentStatus.Active) throw new DomainException("Only an active assignment can be completed.");
        Status = AssignmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }

    public void Cancel()
    {
        if (Status != AssignmentStatus.Active) throw new DomainException("Only an active assignment can be cancelled.");
        Status = AssignmentStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }
}
