using System.Text.RegularExpressions;
using Feezbow.Application.Common.Interfaces;

public partial class RecipeUrlFetcher(IHttpClientFactory httpClientFactory) : IRecipeUrlFetcher
{
    public async Task<string> FetchAsync(
        string url,
        int maxChars = 12000,
        CancellationToken cancellationToken = default
    )
    {
        using var client = httpClientFactory.CreateClient("RecipeUrlFetcher");
        var html = await client.GetStringAsync(url, cancellationToken);

        var noScripts = ScriptStyleRegex().Replace(html, " ");
        var noTags = TagRegex().Replace(noScripts, " ");
        var collapsed = WhitespaceRegex().Replace(noTags, " ").Trim();

        return collapsed.Length > maxChars ? collapsed[..maxChars] : collapsed;
    }

    [GeneratedRegex(@"<(script|style)[^>]*>[\s\S]*?</(script|style)>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptStyleRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex WhitespaceRegex();
}
