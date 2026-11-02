namespace PawRescue.Domain.Common;

/// <summary>
/// Thrown when a business rule inside an entity is violated
/// (e.g. an illegal report status transition).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
