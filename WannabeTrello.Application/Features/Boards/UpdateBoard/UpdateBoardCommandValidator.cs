using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandValidator: AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Board Id is required.");
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Board Name is required.")
            .MaximumLength(100).WithMessage("Board Name cannot exceed 100 characters.");
        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Board Description cannot exceed 500 characters.");
    }
}