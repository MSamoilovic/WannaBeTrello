using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.AssignTaskToUser;

public class AssignTaskToUserCommandValidator : AbstractValidator<AssignTaskToUserCommand>
{
    public AssignTaskToUserCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("TaskId cannot be empty")
            .GreaterThan(0)
            .WithMessage("TaskId must be greater than zero");
        
        RuleFor(x => x.newAssigneeId)
            .NotEmpty()
            .WithMessage("NewAssigneeId cannot be empty")
            .GreaterThan(0)
            .WithMessage("NewAssigneeId must be greater than zero");
    }
}