using FluentValidation;

namespace Feezbow.Application.Features.Labels.DeleteLabel;

public class DeleteLabelCommandValidator : AbstractValidator<DeleteLabelCommand>
{
    public DeleteLabelCommandValidator()
    {
        RuleFor(x => x.LabelId).GreaterThan(0).WithMessage("LabelId must be greater than 0.");
    }
}
