using PawRescue.Application.DTOs;
using PawRescue.Domain.Entities;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Common;

/// <summary>Hand-written mapping (no AutoMapper) so students can see exactly what goes to the client.</summary>
public static class Mappings
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.FullName, u.Email, u.PhoneNumber, u.ProfileImageUrl,
        u.Role?.Name ?? string.Empty, u.IsActive, u.CreatedAt);

    public static AssignmentDto ToDto(this RescueAssignment a) => new(
        a.Id, a.ReportId, a.Report?.Title ?? string.Empty,
        a.VolunteerId, a.Volunteer?.FullName ?? string.Empty,
        a.Status, a.AssignedAt, a.CompletedAt, a.Notes);

    public static ReportDto ToDto(this AnimalReport r) => new(
        r.Id, r.Title, r.Description, r.AnimalType, r.Condition, r.Severity, r.Status, r.PriorityScore,
        new LocationDto(r.Location.Latitude, r.Location.Longitude, r.Location.Address, r.Location.City),
        r.Images.OrderByDescending(i => i.IsPrimary).Select(i => new ImageDto(i.Id, i.Url, i.IsPrimary)).ToList(),
        r.ReporterId, r.Reporter?.FullName ?? string.Empty,
        r.Assignments.FirstOrDefault(a => a.Status == AssignmentStatus.Active)?.ToDto(),
        r.CreatedAt, r.UpdatedAt);

    public static ReportSummaryDto ToSummary(this AnimalReport r, double? distanceKm = null) => new(
        r.Id, r.Title, r.AnimalType, r.Condition, r.Severity, r.Status, r.PriorityScore,
        r.Location.Latitude, r.Location.Longitude, r.Location.Address,
        r.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? r.Images.FirstOrDefault()?.Url,
        distanceKm is null ? null : Math.Round(distanceKm.Value, 2),
        r.CreatedAt);

    public static NotificationDto ToDto(this Notification n) => new(
        n.Id, n.Type, n.Title, n.Message, n.ReportId, n.IsRead, n.CreatedAt);
}
