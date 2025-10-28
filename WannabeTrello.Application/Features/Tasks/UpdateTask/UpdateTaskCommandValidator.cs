using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(v => v.TaskId)
            .NotEmpty().WithMessage("Task ID is required.")
            .GreaterThan(0).WithMessage("Task ID must be greater than 0.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title cannot exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(v => v.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Due date cannot be in the past.");
    }
}