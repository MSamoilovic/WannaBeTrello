using FluentValidation;

namespace Feezbow.Application.Features.Tasks.ArchiveTask;

public class ArchiveTaskCommandValidator : AbstractValidator<ArchiveTaskCommand>
{
    public ArchiveTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().GreaterThan(0).WithMessage("TaskId must be greater than zero");
    }
}