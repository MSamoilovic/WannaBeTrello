using FluentValidation;

namespace WannabeTrello.Application.Features.Users.UpdateUserProfile;

public class UpdateUserProfileCommandValidator: AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().NotNull().GreaterThan(0).WithMessage("UserId must be greater than 0");
        RuleFor(x => x.FirstName).MaximumLength(100).WithMessage("First Name length cannot exceed 100 characters");
        RuleFor(x => x.LastName).MaximumLength(100).WithMessage("Last Name length cannot exceed 100 characters");
        RuleFor(x => x.Bio).MaximumLength(300).WithMessage("Bio cannot exceed 300 characters");
    }
}
