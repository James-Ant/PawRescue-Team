using PawRescue.Application.DTOs;
using PawRescue.Domain.Entities;

namespace PawRescue.Application.Interfaces;

// Abstractions implemented in Infrastructure / Api (DEPENDENCY INVERSION — the "D" in SOLID).

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, string roleName);
    string GenerateRefreshToken();
    int RefreshTokenDays { get; }
    /// <summary>SHA-256 hash used for storing refresh / reset tokens.</summary>
    string HashToken(string token);
}

public record StoredImage(string Url, string StorageKey);

/// <summary>
/// POLYMORPHISM: LocalImageStorageService (dev) and CloudinaryImageStorageService (prod)
/// both implement this. ReportService never knows which one it is using.
/// </summary>
public interface IImageStorageService
{
    Task<StoredImage> UploadAsync(ImageUpload image, string folder, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}

public interface IPushNotificationSender
{
    Task SendAsync(string deviceToken, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default);
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

/// <summary>Strategy pattern for computing a report's 0–100 priority score.</summary>
public interface IPriorityStrategy
{
    int Calculate(AnimalReport report);
}

public record HealthAnalysisResult(string PredictedCondition, double Confidence, string? Notes);

/// <summary>FUTURE FEATURE: AI animal health detection from image (see README).</summary>
public interface IAnimalHealthAnalyzer
{
    Task<HealthAnalysisResult> AnalyzeAsync(string imageUrl, CancellationToken ct = default);
}
