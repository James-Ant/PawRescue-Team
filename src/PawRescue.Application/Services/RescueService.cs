using PawRescue.Application.Common;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;
using PawRescue.Application.Notifications;
using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Services;

public class RescueService : IRescueService
{
    private readonly IReportRepository _reports;
    private readonly IRescueAssignmentRepository _assignments;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _audit;

    public RescueService(IReportRepository reports, IRescueAssignmentRepository assignments, IUserRepository users,
                         IUnitOfWork uow, INotificationService notifications, ICurrentUserService currentUser,
                         IAuditLogger audit)
    {
        _reports = reports;
        _assignments = assignments;
        _users = users;
        _uow = uow;
        _notifications = notifications;
        _currentUser = currentUser;
        _audit = audit;
    }

    private Guid CurrentUserId => _currentUser.UserId ?? throw new UnauthorizedException("Not logged in.");

    public async Task<IReadOnlyList<ReportSummaryDto>> GetAvailableCasesAsync(double? latitude, double? longitude, CancellationToken ct = default)
    {
        var reports = await _reports.GetAvailableForRescueAsync(100, ct);

        if (latitude is null || longitude is null)
            return reports.Select(r => r.ToSummary()).ToList();

        return reports
            .Select(r => r.ToSummary(GeoCalculator.DistanceKm(latitude.Value, longitude.Value, r.Location.Latitude, r.Location.Longitude)))
            .OrderByDescending(r => r.PriorityScore)
            .ThenBy(r => r.DistanceKm)
            .ToList();
    }

    public async Task<AssignmentDto> AcceptAsync(Guid reportId, CancellationToken ct = default)
    {
        var report = await _reports.GetDetailsAsync(reportId, ct) ?? throw new NotFoundException("Report", reportId);

        if (await _assignments.GetActiveForReportAsync(reportId, ct) is not null)
            throw new ConflictException("Another volunteer has already accepted this rescue.");

        // Capstone rule: a volunteer can handle at most 3 active rescues at once.
        if (await _assignments.CountByVolunteerAsync(CurrentUserId, AssignmentStatus.Active, ct) >= 3)
            throw new ValidationException("You already have 3 active rescues. Complete one before accepting another.");

        report.ChangeStatus(ReportStatus.RescueAssigned); // throws DomainException if not allowed
        var assignment = new RescueAssignment(reportId, CurrentUserId);
        await _assignments.AddAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);

        var volunteer = await _users.GetByIdAsync(CurrentUserId, ct);
        await _notifications.NotifyAsync(report.ReporterId,
            new RescueAcceptedNotification(report.Id, report.Title, volunteer?.FullName ?? "A volunteer"), ct);
        await _audit.LogAsync("RESCUE_ACCEPTED", nameof(RescueAssignment), assignment.Id.ToString(), new { reportId }, ct);

        var saved = await _assignments.GetActiveForReportAsync(reportId, ct);
        return saved!.ToDto();
    }

    public async Task<ReportDto> UpdateStatusAsync(Guid reportId, UpdateStatusRequest request, CancellationToken ct = default)
    {
        var report = await _reports.GetDetailsAsync(reportId, ct) ?? throw new NotFoundException("Report", reportId);
        var active = report.Assignments.FirstOrDefault(a => a.Status == AssignmentStatus.Active);
        var isAdmin = _currentUser.IsInRole(RoleNames.Administrator);

        // Only the assigned volunteer (or an admin) can move a case forward.
        if (!isAdmin && active?.VolunteerId != CurrentUserId)
            throw new ForbiddenException("Only the assigned volunteer or an administrator can update this rescue.");

        report.ChangeStatus(request.Status);
        active?.AddNote(request.Note);

        switch (request.Status)
        {
            case ReportStatus.Rescued:
                active?.Complete();
                break;
            case ReportStatus.Closed when active is not null:
            case ReportStatus.UnderReview when active is not null: // volunteer dropped the case
                active!.Cancel();
                break;
        }

        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(report.ReporterId,
            new StatusChangedNotification(report.Id, report.Title, report.Status), ct);
        await _audit.LogAsync("REPORT_STATUS_CHANGED", nameof(AnimalReport), reportId.ToString(),
            new { NewStatus = request.Status.ToString(), request.Note }, ct);

        return report.ToDto();
    }

    public async Task<IReadOnlyList<AssignmentDto>> GetMyAssignmentsAsync(bool activeOnly, CancellationToken ct = default)
    {
        var items = await _assignments.GetByVolunteerAsync(CurrentUserId, activeOnly ? AssignmentStatus.Active : null, ct);
        return items.Select(a => a.ToDto()).ToList();
    }
}
