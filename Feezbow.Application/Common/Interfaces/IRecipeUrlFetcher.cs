namespace Feezbow.Application.Common.Interfaces;

public interface IRecipeUrlFetcher
{
    Task<string> FetchAsync(
        string url,
        int maxChars = 12_000,
        CancellationToken cancellationToken = default
    );
}
