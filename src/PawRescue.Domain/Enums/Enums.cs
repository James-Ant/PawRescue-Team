namespace PawRescue.Domain.Enums;

public enum ReportStatus
{
    Reported = 0,
    UnderReview = 1,
    RescueAssigned = 2,
    InProgress = 3,
    Rescued = 4,
    Closed = 5
}

public enum SeverityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum AnimalType
{
    Dog = 0,
    Cat = 1,
    Bird = 2,
    Other = 3
}

public enum AnimalCondition
{
    Starving = 0,
    Injured = 1,
    Abandoned = 2,
    Sick = 3,
    Lost = 4,
    Other = 5
}

public enum NotificationType
{
    RescueAccepted = 0,
    StatusChanged = 1,
    NewNearbyReport = 2,
    System = 3
}

public enum AssignmentStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

/// <summary>Role names are stored in the Roles table; these constants keep them consistent.</summary>
public static class RoleNames
{
    public const string User = "User";
    public const string Volunteer = "Volunteer";
    public const string Administrator = "Administrator";
}
