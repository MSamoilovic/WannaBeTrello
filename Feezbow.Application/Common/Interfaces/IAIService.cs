namespace Feezbow.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<string> CompleteWithImagesAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput> images,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}

public record AIImageInput(byte[] Data, string MediaType);

public record AIRequestOptions(
    int MaxInputTokens = 8_000,
    int MaxOutputTokens = 1_024,
    bool AllowCaching = true);