using FluentValidation;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandValidator: AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(v => v.ColumnId)
            .NotEmpty().WithMessage("ID kolone je obavezan.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Naslov Taska je obavezan.")
            .MaximumLength(200).WithMessage("Naslov Taska ne sme prelaziti 200 karaktera.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Opis Taska ne sme prelaziti 1000 karaktera.");

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Neispravan prioritet taska.");

        RuleFor(v => v.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Rok za završetak zadatka ne može biti u prošlosti.")
            .When(v => v.DueDate != default); 
    }
}
