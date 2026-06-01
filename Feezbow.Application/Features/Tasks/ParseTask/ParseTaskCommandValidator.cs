using FluentValidation;

namespace Feezbow.Application.Features.Tasks.ParseTask;

public class ParseTaskCommandValidator : AbstractValidator<ParseTaskCommand>
{
    public ParseTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.FreeText).NotEmpty().MaximumLength(1_000);
    }
}
