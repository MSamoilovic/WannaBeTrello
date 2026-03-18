using FluentValidation;

namespace WannabeTrello.Application.Features.Activities.GetActivityByUser;

public class GetActivityByUserQueryValidator : AbstractValidator<GetActivityByUserQuery>
{
    public GetActivityByUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty")
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

