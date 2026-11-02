using PawRescue.Domain.Common;

namespace PawRescue.Domain.Entities;

/// <summary>
/// ENCAPSULATION: setters are private; state only changes through
/// intention-revealing methods (Deactivate, ChangeRole, UpdateLastKnownLocation...).
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    // Last known position — used for "New nearby report" notifications
    public double? LastLatitude { get; private set; }
    public double? LastLongitude { get; private set; }

    // Push notification token (FCM / APNs)
    public string? DeviceToken { get; private set; }

    // Forgot-password flow (only the hash of the token is stored)
    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetExpiresAt { get; private set; }

    public ICollection<AnimalReport> Reports { get; private set; } = new List<AnimalReport>();
    public ICollection<RescueAssignment> Assignments { get; private set; } = new List<RescueAssignment>();

    private User() { } // EF Core

    public User(string fullName, string email, string passwordHash, int roleId, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        RoleId = roleId;
        PhoneNumber = phoneNumber;
    }

    public void UpdateProfile(string fullName, string? phoneNumber, string? profileImageUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Full name is required.");
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        Touch();
    }

    public void ChangePassword(string newHash)
    {
        PasswordHash = newHash;
        PasswordResetTokenHash = null;
        PasswordResetExpiresAt = null;
        Touch();
    }

    public void SetPasswordResetToken(string tokenHash, DateTime expiresAt)
    {
        PasswordResetTokenHash = tokenHash;
        PasswordResetExpiresAt = expiresAt;
        Touch();
    }

    public bool IsPasswordResetTokenValid(string tokenHash) =>
        PasswordResetTokenHash == tokenHash &&
        PasswordResetExpiresAt.HasValue &&
        PasswordResetExpiresAt.Value > DateTime.UtcNow;

    public void ChangeRole(int roleId) { RoleId = roleId; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    public void UpdateLastKnownLocation(double latitude, double longitude)
    {
        LastLatitude = latitude;
        LastLongitude = longitude;
        Touch();
    }

    public void RegisterDevice(string? deviceToken) { DeviceToken = deviceToken; Touch(); }
}
