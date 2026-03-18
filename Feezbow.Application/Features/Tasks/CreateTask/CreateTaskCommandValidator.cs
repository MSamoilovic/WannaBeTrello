using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandValidator: AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(v => v.ColumnId)
            .NotEmpty().WithMessage("ColumnId is required");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title cannot exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Task Description cannot exceed 1000 characters.");

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(v => v.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Due date cannot be in the past.")
            .When(v => v.DueDate != default); 
    }
}
