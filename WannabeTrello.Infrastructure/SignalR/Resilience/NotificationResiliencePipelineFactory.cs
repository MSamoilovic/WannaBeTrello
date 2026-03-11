using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace WannabeTrello.Infrastructure.SignalR.Resilience;

public static class NotificationResiliencePipelineFactory
{
    public static ResiliencePipeline Create(ILogger logger) =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "SignalR notification retry {Attempt}/{Max} after {Delay}ms",
                        args.AttemptNumber + 1,
                        3,
                        args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogError(
                        "SignalR circuit breaker opened – notifications paused for {Duration}s",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("SignalR circuit breaker closed – notifications resumed");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
}
