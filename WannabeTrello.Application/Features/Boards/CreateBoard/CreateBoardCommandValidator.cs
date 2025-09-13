using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public class CreateBoardCommandValidator: AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(v => v.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required.");
    }
}