using System.ComponentModel.DataAnnotations;

namespace PawRescue.Application.DTOs;

public record RegisterRequest(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [MaxLength(20)] string? PhoneNumber,
    bool RegisterAsVolunteer = false);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? DeviceToken = null);

public record RefreshTokenRequest([Required] string RefreshToken);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string Token,
    [Required, MinLength(8)] string NewPassword);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    UserDto User);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

public record UpdateProfileRequest(
    [Required, MaxLength(120)] string FullName,
    [MaxLength(20)] string? PhoneNumber);

public record UpdateLocationRequest(
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    string? DeviceToken);
