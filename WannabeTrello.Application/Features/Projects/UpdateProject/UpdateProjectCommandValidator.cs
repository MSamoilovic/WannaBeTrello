using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.UpdateProject;

public class UpdateProjectCommandValidator: AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required")
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than zero");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}