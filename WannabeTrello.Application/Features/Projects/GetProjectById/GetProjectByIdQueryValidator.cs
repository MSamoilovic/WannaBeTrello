using System.Data;
using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.GetProjectById;

public class GetProjectByIdQueryValidator: AbstractValidator<GetProjectByIdQuery>
{
    public GetProjectByIdQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project Id is required.");
    }
}