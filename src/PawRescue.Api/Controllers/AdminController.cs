using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Enums;

namespace PawRescue.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleNames.Administrator)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IReportService _reports;

    public AdminController(IAdminService admin, IReportService reports)
    {
        _admin = admin;
        _reports = reports;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> Stats(CancellationToken ct)
        => Ok(await _admin.GetStatsAsync(ct));

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<UserDto>>> Users(
        [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _admin.GetUsersAsync(keyword, page, pageSize, ct));

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<UserDto>> ChangeRole(Guid id, ChangeRoleRequest request, CancellationToken ct)
        => Ok(await _admin.ChangeRoleAsync(id, request.Role, ct));

    [HttpPatch("users/{id:guid}/activate")]
    public async Task<ActionResult<UserDto>> Activate(Guid id, CancellationToken ct)
        => Ok(await _admin.SetActiveAsync(id, true, ct));

    [HttpPatch("users/{id:guid}/deactivate")]
    public async Task<ActionResult<UserDto>> Deactivate(Guid id, CancellationToken ct)
        => Ok(await _admin.SetActiveAsync(id, false, ct));

    /// <summary>Report management uses the same search as the map, without the "open only" restriction.</summary>
    [HttpGet("reports")]
    public async Task<ActionResult<PagedResult<ReportSummaryDto>>> Reports([FromQuery] SearchQuery query, CancellationToken ct)
        => Ok(await _reports.SearchAsync(query, ct));

    [HttpGet("activity")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> Activity(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
        => Ok(await _admin.GetActivityAsync(page, pageSize, ct));

    /// <summary>FUTURE FEATURE: analytics heatmap data. Returns 501 for now.</summary>
    [HttpGet("heatmap")]
    public IActionResult Heatmap()
        => StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Analytics heatmap is planned for a future iteration (see README → Unfinished features)." });
}
