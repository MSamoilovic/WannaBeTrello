using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.GetBoardsByProjectId;

public class GetBoardsByProjectIdQueryValidator : AbstractValidator<GetBoardsByProjectIdQuery>
{
    public GetBoardsByProjectIdQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("ProjectId must be greater than zero");
    }
}