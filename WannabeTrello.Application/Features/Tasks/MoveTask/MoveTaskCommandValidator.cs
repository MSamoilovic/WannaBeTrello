using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

public class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskCommandValidator()
    {
        RuleFor(v => v.TaskId)
            .NotEmpty().WithMessage("ID Taska je obavezan.");

        RuleFor(v => v.NewColumnId)
            .NotEmpty().WithMessage("ID nove kolone je obavezan.");
    }
}