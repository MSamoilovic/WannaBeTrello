using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.AddCommentToTask;

public class AddCommentToTaskCommandValidator : AbstractValidator<AddCommentToTaskCommand>
{
    public AddCommentToTaskCommandValidator()
    {
        RuleFor(v => v.TaskId)
            .NotEmpty().WithMessage("ID Taska je obavezan.");
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Sadržaj komentara je obavezan.")
            .MaximumLength(1000).WithMessage("Sadržaj komentara ne sme prelaziti 1000 karaktera.");
    }
}