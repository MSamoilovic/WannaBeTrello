using FluentValidation;

namespace WannabeTrello.Application.Features.Labels.CreateLabel;

public class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator()
    {
        RuleFor(x => x.BoardId).GreaterThan(0).WithMessage("BoardId must be greater than 0.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50).WithMessage("Label name is required and cannot exceed 50 characters.");
        RuleFor(x => x.Color).NotEmpty().Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex color (e.g., #FF5733).");
    }
}
