namespace Velocify.Domain.Exceptions;

/// <summary>
/// Exception thrown when a request contains invalid data or business rule violations.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : DomainException
{
    public BadRequestException(string message) : base(message)
    {
    }
}
