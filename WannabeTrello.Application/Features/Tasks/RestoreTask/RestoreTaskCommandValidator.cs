using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.RestoreTask;

public class RestoreTaskCommandValidator : AbstractValidator<RestoreTaskCommand>
{
    public RestoreTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().GreaterThan(0).WithMessage("TaskId is required");
    }
}