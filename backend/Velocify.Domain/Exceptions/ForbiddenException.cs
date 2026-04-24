namespace Velocify.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user is authenticated but lacks permission to perform an action.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message)
    {
    }
}
