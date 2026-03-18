using FluentValidation;

namespace WannabeTrello.Application.Features.Labels.AddLabelToTask;

public class AddLabelToTaskCommandValidator : AbstractValidator<AddLabelToTaskCommand>
{
    public AddLabelToTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0).WithMessage("TaskId must be greater than 0.");
        RuleFor(x => x.LabelId).GreaterThan(0).WithMessage("LabelId must be greater than 0.");
    }
}
