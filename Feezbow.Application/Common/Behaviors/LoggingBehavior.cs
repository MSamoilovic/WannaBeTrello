using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Feezbow.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command", StringComparison.Ordinal);

        logger.LogDebug("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        sw.Stop();

        if (isCommand)
            logger.LogInformation("{RequestName} completed in {ElapsedMs} ms", requestName, sw.ElapsedMilliseconds);
        else
            logger.LogDebug("{RequestName} completed in {ElapsedMs} ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
