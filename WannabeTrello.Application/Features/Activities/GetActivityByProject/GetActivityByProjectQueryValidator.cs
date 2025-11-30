using FluentValidation;

namespace WannabeTrello.Application.Features.Activities.GetActivityByProject;

internal class GetActivityByProjectQueryValidator: AbstractValidator<GetActivityByProjectQuery>
{
    public GetActivityByProjectQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty")
            .GreaterThan(0)
            .WithMessage("BoardId must be greather than 0");
    }
}
