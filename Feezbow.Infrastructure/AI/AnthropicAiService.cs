using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Infrastructure.Options;
using Feezbow.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feezbow.Infrastructure.AI;

public class AnthropicAiService(
    AnthropicClient client,
    IOptions<AnthropicOptions> options,
    ICurrentUserService currentUserService,
    ILogger<AnthropicAiService> logger,
    ApplicationDbContext dbContext
) : IAIService
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
    ) => SendAsync(systemPrompt, userPrompt, images: null, options, cancellationToken);

    public Task<string> CompleteWithImagesAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput> images,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default
    ) => SendAsync(systemPrompt, userPrompt, images, options, cancellationToken);

    private async Task<string> SendAsync(
        string systemPrompt,
        string userPrompt,
        IReadOnlyList<AIImageInput>? images,
        AIRequestOptions? opts,
        CancellationToken ct
    )
    {
        opts ??= new AIRequestOptions();

        if (!options.Value.Enabled)
            throw new AIServiceException("AI service is disabled.");

        if (opts.RequiresFeature == "RecipeParser" && !options.Value.RecipeParserEnabled)
            throw new AIServiceException("Recipe parser feature is disabled.");

        // Token budget guard --- english => chars/4
        int estimatedTokens = (int)Math.Ceiling((systemPrompt.Length + userPrompt.Length) / 4.0);
        if (estimatedTokens > opts.MaxInputTokens)
            throw new AITokenBudgetExceededException(estimatedTokens, opts.MaxInputTokens);

        var content = new List<ContentBase>();

        //Convention - Images before text

        if (images != null)
        {
            foreach (AIImageInput img in images)
            {
                content.Add(
                    new ImageContent
                    {
                        Source = new ImageSource
                        {
                            MediaType = img.MediaType,
                            Data = Convert.ToBase64String(img.Data),
                        },
                    }
                );
            }
        }

        content.Add(new TextContent { Text = userPrompt });

        var parameters = new MessageParameters
        {
            Model = options.Value.DefaultModel,
            MaxTokens = opts.MaxOutputTokens,
            System = [new SystemMessage(systemPrompt)],
            Messages = [new Message { Role = RoleType.User, Content = content }],
        };

        if (options.Value.PromptCachingEnabled && opts.AllowCaching)
            parameters.PromptCaching = PromptCacheType.AutomaticToolsAndSystem;

        MessageResponse response;
        try
        {
            response = await client.Messages.GetClaudeMessageAsync(parameters, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Anthropic API call failed for model {Model}",
                options.Value.DefaultModel
            );
            throw new AIServiceException("AI service call failed.", ex);
        }

        var inputTokens = response.Usage?.InputTokens ?? 0;
        var outputTokens = response.Usage?.OutputTokens ?? 0;
        var cacheHit = (response.Usage?.CacheReadInputTokens ?? 0) > 0;

        logger.LogInformation(
            "AI call complete. Model={Model} In={In} Out={Out} CacheHit={CacheHit}",
            options.Value.DefaultModel,
            inputTokens,
            outputTokens,
            cacheHit
        );

        try
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userPrompt)))[
                ..16
            ];
            var log = AiAuditLog.Create(
                currentUserService.UserId,
                "AnthropicAIService",
                hash,
                inputTokens,
                outputTokens,
                cacheHit
            );

            dbContext.AiAuditLogs.Add(log);

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write AI audit log");
        }

        return response.Content.FirstOrDefault()?.ToString() ?? string.Empty;
    }

    public static T ParseOrThrow<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions)!;
        }
        catch
        {
            throw new AIResponseParseException(typeof(T).Name);
        }
    }
}
