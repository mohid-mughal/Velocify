namespace Velocify.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with the current state of the resource.
/// Used for concurrency conflicts and duplicate resource errors.
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConflictException : DomainException
{
    public object? CurrentValues { get; }

    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, object currentValues) : base(message)
    {
        CurrentValues = currentValues;
    }
}
