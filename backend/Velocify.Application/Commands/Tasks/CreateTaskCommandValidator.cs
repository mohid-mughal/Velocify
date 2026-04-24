using FluentValidation;

namespace Velocify.Application.Commands.Tasks;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid value");

        RuleFor(x => x.AssignedToUserId)
            .NotEmpty().WithMessage("AssignedToUserId is required");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).When(x => x.EstimatedHours.HasValue)
            .WithMessage("EstimatedHours must be greater than 0");
    }
}
