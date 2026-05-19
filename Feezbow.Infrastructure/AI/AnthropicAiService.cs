using System.Text.Json;
using Feezbow.Application.Common.Interfaces;

namespace Feezbow.Infrastructure.AI;

public class AnthropicAiService() : IAIService
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default
    ) => throw new NotImplementedException();

    public Task<string> CompleteWithImagesAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput> images,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
