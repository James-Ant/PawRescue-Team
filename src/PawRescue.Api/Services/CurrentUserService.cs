using PawRescue.Application.Interfaces;

namespace PawRescue.Api.Services;

/// <summary>Reads the logged-in user from the validated JWT on the current request.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;
    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public Guid? UserId =>
        Guid.TryParse(_http.HttpContext?.User.FindFirst("sub")?.Value, out var id) ? id : null;

    public string? Role => _http.HttpContext?.User.FindFirst("role")?.Value;

    public string? IpAddress => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsAuthenticated => _http.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => _http.HttpContext?.User.IsInRole(role) == true;
}
