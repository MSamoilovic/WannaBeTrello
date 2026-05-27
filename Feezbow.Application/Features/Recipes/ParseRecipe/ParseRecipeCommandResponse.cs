namespace Feezbow.Application.Features.Recipes.ParseRecipe;

public record ParseRecipeCommandResponse(ParsedRecipeDto Recipe);

public record ParsedRecipeDto(
    string? Name,
    int? Servings,
    IReadOnlyList<ParsedIngredientDto> Ingredients,
    IReadOnlyList<string> Steps
);

public record ParsedIngredientDto(string Name, double? Quantity, string? Unit);
