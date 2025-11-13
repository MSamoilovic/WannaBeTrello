using FluentValidation;

namespace WannabeTrello.Application.Features.Users.GetUserProjects;

public class GetUserProjectsQueryValidator : AbstractValidator<GetUserProjectsQuery>
{
    public GetUserProjectsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

