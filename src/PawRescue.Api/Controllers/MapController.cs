using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;

namespace PawRescue.Api.Controllers;

[ApiController]
[Route("api/map")]
[Authorize]
public class MapController : ControllerBase
{
    private readonly IReportService _reports;
    public MapController(IReportService reports) => _reports = reports;

    /// <summary>
    /// Map pins around a point.
    /// Example: GET /api/map/nearby?latitude=14.81&amp;longitude=121.05&amp;radiusKm=5&amp;minSeverity=High
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyList<ReportSummaryDto>>> Nearby([FromQuery] NearbyQuery query, CancellationToken ct)
        => Ok(await _reports.GetNearbyAsync(query, ct));

    /// <summary>Keyword + filter search with paging. Example: GET /api/map/search?keyword=dog&amp;status=Reported</summary>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ReportSummaryDto>>> Search([FromQuery] SearchQuery query, CancellationToken ct)
        => Ok(await _reports.SearchAsync(query, ct));
}
