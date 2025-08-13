using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.ArchiveProject;

public class ArchiveProjectCommandValidator: AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull()
            .WithMessage("Project ID cannot be null");
    }
}