using FluentValidation;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public class DeleteColumnCommandValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnCommandValidator()
    {
        RuleFor(x => x.ColumnId)
            .GreaterThan(0)
            .WithMessage("Column ID must be greater than zero");
    }
}