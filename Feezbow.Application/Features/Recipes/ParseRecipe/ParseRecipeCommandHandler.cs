using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using MediatR;

namespace Feezbow.Application.Features.Recipes.ParseRecipe;

public class ParseRecipeCommandHandler(IAIService aiService, IRecipeUrlFetcher urlFetcher)
    : IRequestHandler<ParseRecipeCommand, ParseRecipeCommandResponse>
{
    private const string SystemPrompt = """
        You are a recipe extraction assistant. Given the HTML or plain text of a recipe page, extract:
        - Recipe name
        - Serving count (integer)
        - Ingredients: name, quantity (numeric), unit (standardize to: g, kg, ml, l, pcs, tbsp, tsp, cup)
        - Preparation steps (ordered list of plain strings)

        Return ONLY valid JSON matching this exact schema:
        {
          "name": string,
          "servings": number,
          "ingredients": [{ "name": string, "quantity": number, "unit": string }],
          "steps": [string]
        }

        If a field cannot be determined, omit it or use null. Never invent data.
        """;

    private record RawRecipe(
        string? Name,
        int? Servings,
        List<RawIngredient>? Ingredients,
        List<string>? Steps
    );

    private record RawIngredient(string? Name, double? Quantity, string? Unit);

    public async Task<ParseRecipeCommandResponse> Handle(
        ParseRecipeCommand request,
        CancellationToken cancellationToken
    )
    {
        string pageText;
        if (!string.IsNullOrWhiteSpace(request.Url))
            pageText = await urlFetcher.FetchAsync(
                request.Url,
                maxChars: 12_000,
                cancellationToken
            );
        else
            pageText = request.RawText!;

        var json = await aiService.CompleteAsync(
            SystemPrompt,
            pageText,
            new AIRequestOptions(
                MaxInputTokens: 16_000,
                MaxOutputTokens: 2_048,
                AllowCaching: true,
                RequiresFeature: "RecipeParser"
            ),
            cancellationToken
        );

        // Strip markdown code fences if present
        json = json.Trim();
        if (json.StartsWith("```"))
            json = string.Join('\n', json.Split('\n')[1..^1]);

        RawRecipe raw;
        try
        {
            raw = System.Text.Json.JsonSerializer.Deserialize<RawRecipe>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
        catch
        {
            throw new AIResponseParseException(nameof(ParsedRecipeDto));
        }

        var dto = new ParsedRecipeDto(
            raw.Name,
            raw.Servings,
            raw.Ingredients?.Select(i => new ParsedIngredientDto(i.Name ?? "", i.Quantity, i.Unit))
                .ToList()
                ?? [],
            raw.Steps ?? []
        );

        return new ParseRecipeCommandResponse(dto);
    }
}
