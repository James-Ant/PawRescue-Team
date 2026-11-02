using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawRescue.Application.DTOs;
using PawRescue.Application.Interfaces;

namespace PawRescue.Api.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    /// <summary>Profile + contribution stats.</summary>
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get(CancellationToken ct)
        => Ok(await _users.GetMyProfileAsync(ct));

    [HttpPut]
    public async Task<ActionResult<UserDto>> Update(UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _users.UpdateMyProfileAsync(request, ct));

    /// <summary>The app calls this on start-up so volunteers receive "new nearby report" alerts.</summary>
    [HttpPut("location")]
    public async Task<IActionResult> UpdateLocation(UpdateLocationRequest request, CancellationToken ct)
    {
        await _users.UpdateMyLocationAsync(request, ct);
        return NoContent();
    }
}
