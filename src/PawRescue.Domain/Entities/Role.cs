namespace PawRescue.Domain.Entities;

/// <summary>
/// Roles use a small int key because they are seeded lookup data.
/// (Not derived from BaseEntity on purpose: no Guid, no timestamps needed.)
/// </summary>
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}

/// <summary>Seeded role ids (must match the HasData seed in RoleConfiguration).</summary>
public static class RoleIds
{
    public const int User = 1;
    public const int Volunteer = 2;
    public const int Administrator = 3;
}
