using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryValidator : AbstractValidator<GetBoardByIdQuery>
{
    public GetBoardByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID boarda je obavezan.");
    }
}