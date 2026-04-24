namespace Velocify.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication is required but not provided or invalid.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Authentication is required to access this resource.")
        : base(message)
    {
    }
}
