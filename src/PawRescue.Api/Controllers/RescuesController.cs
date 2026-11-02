using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Enums;

namespace PawRescue.Api.Controllers;

[ApiController]
[Route("api/rescues")]
[Authorize(Roles = $"{RoleNames.Volunteer},{RoleNames.Administrator}")]
public class RescuesController : ControllerBase
{
    private readonly IRescueService _rescues;
    public RescuesController(IRescueService rescues) => _rescues = rescues;

    /// <summary>Open cases waiting for a volunteer, sorted by priority then distance.</summary>
    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<ReportSummaryDto>>> Available(
        [FromQuery] double? latitude, [FromQuery] double? longitude, CancellationToken ct)
        => Ok(await _rescues.GetAvailableCasesAsync(latitude, longitude, ct));

    [HttpPost("{reportId:guid}/accept")]
    public async Task<ActionResult<AssignmentDto>> Accept(Guid reportId, CancellationToken ct)
        => Ok(await _rescues.AcceptAsync(reportId, ct));

    /// <summary>Move a case along: InProgress → Rescued → Closed (with an optional note).</summary>
    [HttpPatch("{reportId:guid}/status")]
    public async Task<ActionResult<ReportDto>> UpdateStatus(Guid reportId, UpdateStatusRequest request, CancellationToken ct)
        => Ok(await _rescues.UpdateStatusAsync(reportId, request, ct));

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> Mine([FromQuery] bool activeOnly = false, CancellationToken ct = default)
        => Ok(await _rescues.GetMyAssignmentsAsync(activeOnly, ct));
}
