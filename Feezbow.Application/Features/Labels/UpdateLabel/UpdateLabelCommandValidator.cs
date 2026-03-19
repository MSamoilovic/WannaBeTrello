using FluentValidation;

namespace Feezbow.Application.Features.Labels.UpdateLabel;

public class UpdateLabelCommandValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelCommandValidator()
    {
        RuleFor(x => x.LabelId).GreaterThan(0).WithMessage("LabelId must be greater than 0.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50).WithMessage("Label name is required and cannot exceed 50 characters.");
        RuleFor(x => x.Color).NotEmpty().Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Color must be a valid hex color (e.g., #FF5733).");
    }
}
