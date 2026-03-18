using MediatR;
using Microsoft.Extensions.Logging;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            var response = await next(cancellationToken);
            
            var requestTypeName = typeof(TRequest).Name;
            if (!requestTypeName.EndsWith("Command", StringComparison.Ordinal)) return response;
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Transaction confirmed for request {Name}", typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction cancelled for request {Name}", typeof(TRequest).Name);
            throw;
        }
    }
}