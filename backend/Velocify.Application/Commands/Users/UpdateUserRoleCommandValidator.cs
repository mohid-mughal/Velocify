using FluentValidation;
using Velocify.Domain.Enums;

namespace Velocify.Application.Commands.Users;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid UserRole value.");
    }
}
