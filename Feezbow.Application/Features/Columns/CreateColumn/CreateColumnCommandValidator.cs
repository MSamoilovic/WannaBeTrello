using FluentValidation;

namespace WannabeTrello.Application.Features.Columns.CreateColumn;

public class CreateColumnCommandValidator: AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty().NotNull().WithMessage("BoardId is required");
        RuleFor(x => x.BoardId).GreaterThan(0).WithMessage("BoardId must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().NotNull().WithMessage("Name is required");
        RuleFor(x => x.Name).MaximumLength(30).WithMessage("Name must not exceed 30 characters");
    }
}