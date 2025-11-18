using FluentValidation;

namespace WannabeTrello.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandValidator: AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters");
    }
}
