using FluentValidation;

namespace WannabeTrello.Application.Features.Columns.UpdateColumn;

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(x => x.ColumnId).GreaterThan(0).WithMessage("Column ID must be greater than zero");
        
        When(x => x.NewName != null, () =>
        {
            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("Column name cannot be empty.")
                .MaximumLength(100).WithMessage("Column name cannot exceed 100 characters.");
        });
        
        When(x => x.WipLimit.HasValue, () =>
        {
            RuleFor(x => x.WipLimit)
                .GreaterThan(0).WithMessage("WIP Limit must be a positive number.");
        });
    }
}