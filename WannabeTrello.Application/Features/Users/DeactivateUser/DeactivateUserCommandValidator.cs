using FluentValidation;

namespace WannabeTrello.Application.Features.Users.DeactivateUser;

public class DeactivateUserCommandValidator: AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().NotNull().GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}
