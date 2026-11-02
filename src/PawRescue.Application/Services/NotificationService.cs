using Microsoft.Extensions.Logging;
using PawRescue.Application.Common;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;
using PawRescue.Application.Notifications;
using PawRescue.Domain.Entities;

namespace PawRescue.Application.Services;

/// <summary>
/// Stores an in-app notification AND sends a push notification.
/// The in-app list works even if push is not configured.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPushNotificationSender _push;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository notifications, IUserRepository users, IUnitOfWork uow,
                               IPushNotificationSender push, ICurrentUserService currentUser,
                               ILogger<NotificationService> logger)
    {
        _notifications = notifications;
        _users = users;
        _uow = uow;
        _push = push;
        _currentUser = currentUser;
        _logger = logger;
    }

    private Guid CurrentUserId => _currentUser.UserId ?? throw new UnauthorizedException("Not logged in.");

    // ============================================================
    // TODO(MEMBER 2): Return the logged-in user's notifications, newest first, paged.
    // ------------------------------------------------------------
    // 1. Clamp the input: page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    // 2. var (items, total) = await _notifications.GetForUserAsync(CurrentUserId, unreadOnly, page, pageSize, ct);
    //    (you also need to write GetForUserAsync in NotificationRepository)
    // 3. Map with items.Select(n => n.ToDto()) and return new PagedResult<NotificationDto>(...).
    // Hint: AdminService.GetUsersAsync does the same paging pattern.
    // ============================================================
    public Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(bool unreadOnly, int page, int pageSize, CancellationToken ct = default)
        => throw new FeatureNotImplementedException("Notification list (Member 2)");

    // TODO(MEMBER 2): return _notifications.CountUnreadAsync(CurrentUserId, ct);
    // (Returns 0 for now so the Home screen bell still works.)
    public Task<int> GetUnreadCountAsync(CancellationToken ct = default) => Task.FromResult(0);

    // TODO(MEMBER 2): Mark ONE notification as read.
    //   - Load it with _notifications.GetByIdAsync(id, ct) → throw NotFoundException if null.
    //   - If n.UserId != CurrentUserId → throw new ForbiddenException(); (users can't touch others' notifications)
    //   - n.MarkAsRead(); then await _uow.SaveChangesAsync(ct);
    public Task MarkAsReadAsync(Guid id, CancellationToken ct = default)
        => throw new FeatureNotImplementedException("Mark notification as read (Member 2)");

    // TODO(MEMBER 2): await _notifications.MarkAllAsReadAsync(CurrentUserId, ct); then save.
    public Task MarkAllAsReadAsync(CancellationToken ct = default)
        => throw new FeatureNotImplementedException("Mark all notifications as read (Member 2)");

    /// <summary>POLYMORPHISM: accepts any NotificationTemplate subclass.</summary>
    public async Task NotifyAsync(Guid userId, NotificationTemplate template, CancellationToken ct = default)
    {
        var entity = template.ToEntity(userId);
        await _notifications.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var user = await _users.GetByIdAsync(userId, ct);
        await TryPushAsync(user?.DeviceToken, entity, ct);
    }

    public async Task NotifyNearbyVolunteersAsync(AnimalReport report, double radiusKm, CancellationToken ct = default)
    {
        var box = GeoCalculator.BoundingBox(report.Location.Latitude, report.Location.Longitude, radiusKm);
        var volunteers = await _users.GetVolunteersInBoxAsync(box.MinLat, box.MaxLat, box.MinLon, box.MaxLon, ct);

        var created = new List<(User User, Notification Notification)>();
        foreach (var v in volunteers)
        {
            if (v.Id == report.ReporterId || v.LastLatitude is null || v.LastLongitude is null) continue;

            var distance = GeoCalculator.DistanceKm(report.Location.Latitude, report.Location.Longitude,
                                                    v.LastLatitude.Value, v.LastLongitude.Value);
            if (distance > radiusKm) continue;

            var n = new NewNearbyReportNotification(report.Id, report.Title, report.Severity, distance).ToEntity(v.Id);
            await _notifications.AddAsync(n, ct);
            created.Add((v, n));
        }

        if (created.Count == 0) return;
        await _uow.SaveChangesAsync(ct);

        foreach (var (user, n) in created)
            await TryPushAsync(user.DeviceToken, n, ct);

        _logger.LogInformation("Notified {Count} nearby volunteers about report {ReportId}", created.Count, report.Id);
    }

    private async Task TryPushAsync(string? deviceToken, Notification n, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(deviceToken)) return;
        try
        {
            var data = new Dictionary<string, string> { ["type"] = n.Type.ToString() };
            if (n.ReportId is not null) data["reportId"] = n.ReportId.Value.ToString();
            await _push.SendAsync(deviceToken, n.Title, n.Message, data, ct);
        }
        catch (Exception ex)
        {
            // A failed push must never break the main action.
            _logger.LogWarning(ex, "Push notification failed for notification {Id}", n.Id);
        }
    }
}
