using FluentValidation;
using MediatR;

namespace Velocify.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// 
/// PIPELINE POSITION: This behavior MUST execute FIRST in the pipeline.
/// 
/// Order importance:
/// 1. ValidationBehavior (this) - Validates input before any processing
/// 2. LoggingBehavior - Logs only valid requests
/// 3. PerformanceBehavior - Measures only valid request execution time
/// 
/// Rationale: Validation must happen before any business logic executes to ensure
/// data integrity and prevent invalid state changes. If validation runs after logging
/// or performance measurement, we would waste resources processing invalid requests.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators are registered for this request type, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel for better performance
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If there are validation failures, throw ValidationException
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validation passed, continue to next behavior or handler
        return await next();
    }
}
