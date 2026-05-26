using FluentValidation;

namespace Feezbow.Application.Features.Recipes.ParseRecipe;

public class ParseRecipeCommandValidator : AbstractValidator<ParseRecipeCommand>
{
    public ParseRecipeCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Url) || !string.IsNullOrWhiteSpace(x.RawText))
            .WithMessage("Either Url or RawText must be provided.");

        When(
            x => !string.IsNullOrWhiteSpace(x.Url),
            () =>
                RuleFor(x => x.Url!)
                    .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
                    .WithMessage("Url must be a valid absolute URI.")
        );
    }
}
