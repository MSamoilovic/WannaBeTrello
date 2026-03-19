using FluentValidation;

namespace Feezbow.Application.Features.Labels.RemoveLabelFromTask;

public class RemoveLabelFromTaskCommandValidator : AbstractValidator<RemoveLabelFromTaskCommand>
{
    public RemoveLabelFromTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0).WithMessage("TaskId must be greater than 0.");
        RuleFor(x => x.LabelId).GreaterThan(0).WithMessage("LabelId must be greater than 0.");
    }
}
