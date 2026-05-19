namespace Feezbow.Infrastructure.Options;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; init; } = string.Empty;   // placeholder — set in .env
    public string DefaultModel { get; init; } = "claude-opus-4-6";
    public int MaxTokensDefault { get; init; } = 1_024;
    public bool PromptCachingEnabled { get; init; } = true;

    /// <summary>
    /// AI prijedlozi sa confidence ispod ovog praga se ne vraćaju korisniku.
    /// Handler treba provjeriti score i baciti ili logirati ako je ispod praga.
    /// </summary>
    public double MinConfidenceScore { get; init; } = 0.7;
}