using FluentValidation;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public class DeleteColumnCommandValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnCommandValidator()
    {
        RuleFor(x => x.ColumnId).NotNull().NotEmpty().WithMessage("ColumnId is required");
    }
}