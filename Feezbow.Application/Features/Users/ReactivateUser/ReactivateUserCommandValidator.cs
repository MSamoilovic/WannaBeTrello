using FluentValidation;

namespace WannabeTrello.Application.Features.Users.ReactivateUser;

public class ReactivateUserCommandValidator: AbstractValidator<ReactivateUserCommand>
{
    public ReactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().NotNull().GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}
