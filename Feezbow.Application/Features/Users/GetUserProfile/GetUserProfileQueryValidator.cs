using FluentValidation;

namespace WannabeTrello.Application.Features.Users.GetUserProfile;

public class GetUserProfileQueryValidator: AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}
