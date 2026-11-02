namespace PawRescue.Application.Common;

/// <summary>
/// INHERITANCE + POLYMORPHISM:
/// The API's exception middleware only knows about AppException and reads
/// the overridden StatusCode — it never needs a switch on concrete types.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
    public abstract int StatusCode { get; }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string entity, object key) : base($"{entity} '{key}' was not found.") { }
    public override int StatusCode => 404;
}

public sealed class ValidationException : AppException
{
    public ValidationException(string message) : base(message) { }
    public override int StatusCode => 400;
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message) { }
    public override int StatusCode => 403;
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
    public override int StatusCode => 401;
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
    public override int StatusCode => 409;
}

/// <summary>Used by the intentionally-unfinished features (AI detection, etc.).</summary>
public sealed class FeatureNotImplementedException : AppException
{
    public FeatureNotImplementedException(string feature)
        : base($"'{feature}' is planned for a future iteration and is not available yet.") { }
    public override int StatusCode => 501;
}
