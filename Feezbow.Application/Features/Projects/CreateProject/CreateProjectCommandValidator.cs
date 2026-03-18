using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public class CreateProjectCommandValidator: AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne sme prelaziti 100 karaktera.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Opis ne sme prelaziti 500 karaktera.");
    }
}