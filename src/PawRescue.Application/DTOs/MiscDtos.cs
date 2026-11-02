using System.ComponentModel.DataAnnotations;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.DTOs;

public record UpdateStatusRequest(
    ReportStatus Status,
    [MaxLength(500)] string? Note);

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    Guid? ReportId,
    bool IsRead,
    DateTime CreatedAt);

public record UserStatsDto(
    int ReportsSubmitted,
    int ReportsRescued,
    int RescuesCompleted,
    int ActiveRescues);

public record ProfileDto(UserDto User, UserStatsDto Stats);

public record DashboardStatsDto(
    int TotalUsers,
    int TotalVolunteers,
    int TotalReports,
    int OpenReports,
    int RescuedReports,
    int CriticalOpenReports,
    int ReportsLast7Days,
    IReadOnlyDictionary<string, int> ReportsByStatus,
    IReadOnlyDictionary<string, int> ReportsByAnimalType);

public record AuditLogDto(
    Guid Id,
    string? UserName,
    string Action,
    string EntityName,
    string? EntityId,
    string? Details,
    DateTime CreatedAt);

public record ChangeRoleRequest([Required] string Role);
