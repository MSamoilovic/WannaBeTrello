using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandValidator: AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID Borda je obavezan.");
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Naziv Borda je obavezan.")
            .MaximumLength(100).WithMessage("Naziv Borda ne sme prelaziti 100 karaktera.");
        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Opis Borda ne sme prelaziti 500 karaktera.");
    }
}