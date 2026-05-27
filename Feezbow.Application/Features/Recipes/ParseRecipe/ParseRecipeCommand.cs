using MediatR;

namespace Feezbow.Application.Features.Recipes.ParseRecipe;

public record ParseRecipeCommand(long ProjectId, string? Url, string? RawText)
    : IRequest<ParseRecipeCommandResponse>;
