using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public class CreateBoardCommandValidator: AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne sme prelaziti 100 karaktera.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Opis ne sme prelaziti 500 karaktera.");

        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("ID projekta je obavezan.");
    }
}