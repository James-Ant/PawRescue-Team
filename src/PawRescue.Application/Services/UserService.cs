using PawRescue.Application.Common;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Enums;

namespace PawRescue.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IReportRepository _reports;
    private readonly IRescueAssignmentRepository _assignments;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UserService(IUserRepository users, IReportRepository reports, IRescueAssignmentRepository assignments,
                       IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _users = users;
        _reports = reports;
        _assignments = assignments;
        _uow = uow;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId ?? throw new UnauthorizedException("Not logged in.");

    public async Task<ProfileDto> GetMyProfileAsync(CancellationToken ct = default)
    {
        var user = await _users.GetWithRoleAsync(CurrentUserId, ct) ?? throw new NotFoundException("User", CurrentUserId);

        var stats = new UserStatsDto(
            ReportsSubmitted: await _reports.CountByReporterAndStatusAsync(user.Id, null, ct),
            ReportsRescued: await _reports.CountByReporterAndStatusAsync(user.Id, ReportStatus.Rescued, ct),
            RescuesCompleted: await _assignments.CountByVolunteerAsync(user.Id, AssignmentStatus.Completed, ct),
            ActiveRescues: await _assignments.CountByVolunteerAsync(user.Id, AssignmentStatus.Active, ct));

        return new ProfileDto(user.ToDto(), stats);
    }

    public async Task<UserDto> UpdateMyProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetWithRoleAsync(CurrentUserId, ct) ?? throw new NotFoundException("User", CurrentUserId);
        user.UpdateProfile(request.FullName, request.PhoneNumber, null);
        await _uow.SaveChangesAsync(ct);
        return user.ToDto();
    }

    public async Task UpdateMyLocationAsync(UpdateLocationRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(CurrentUserId, ct) ?? throw new NotFoundException("User", CurrentUserId);
        user.UpdateLastKnownLocation(request.Latitude, request.Longitude);
        if (!string.IsNullOrWhiteSpace(request.DeviceToken)) user.RegisterDevice(request.DeviceToken);
        await _uow.SaveChangesAsync(ct);
    }
}
