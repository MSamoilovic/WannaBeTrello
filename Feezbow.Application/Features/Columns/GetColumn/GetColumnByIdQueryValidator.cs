using FluentValidation;

namespace WannabeTrello.Application.Features.Columns.GetColumn;

public class GetColumnByIdQueryValidator : AbstractValidator<GetColumnByIdQuery>
{
    public GetColumnByIdQueryValidator()
    {
        RuleFor(x => x.ColumnId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0).WithMessage("Column Id must be greater than zero");
    }
}