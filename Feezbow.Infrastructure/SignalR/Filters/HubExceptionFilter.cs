using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Infrastructure.SignalR.Filters;

/// <summary>
/// Hub filter that maps domain exceptions to descriptive <see cref="HubException"/> messages.
/// Runs as the innermost filter (registered last) so it intercepts exceptions
/// before generic catch-all filters see them.
/// </summary>
public sealed class HubExceptionFilter(ILogger<HubExceptionFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (HubException)
        {
            throw; // Already converted – pass through
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(
                "Resource not found in {Method}: {Message}",
                invocationContext.HubMethodName, ex.Message);

            throw new HubException(ex.Message);
        }
        catch (AccessDeniedException)
        {
            logger.LogWarning(
                "Access denied in {Method} for user {UserId}",
                invocationContext.HubMethodName,
                invocationContext.Context.UserIdentifier);

            throw new HubException("Access denied.");
        }
        catch (BusinessRuleValidationException ex)
        {
            logger.LogWarning(
                "Business rule violated in {Method}: {Message}",
                invocationContext.HubMethodName, ex.Message);

            throw new HubException(ex.Message);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(
                ex,
                "Domain exception in {Method}",
                invocationContext.HubMethodName);

            throw new HubException(ex.Message);
        }
    }
}
