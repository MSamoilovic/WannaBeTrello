using FluentValidation;

namespace WannabeTrello.Application.Features.Users.GetUserOwnedProjects;

public class GetUserOwnedProjectsQueryValidator : AbstractValidator<GetUserOwnedProjectsQuery>
{
    public GetUserOwnedProjectsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

