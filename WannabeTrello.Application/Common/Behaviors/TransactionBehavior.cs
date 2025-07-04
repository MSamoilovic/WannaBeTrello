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
            
            await unitOfWork.CompleteAsync(cancellationToken);

            logger.LogInformation($"Transakcija potvrđena za zahtev {typeof(TRequest).Name}");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Transakcija poništena za zahtev {typeof(TRequest).Name}");
            throw;
        }
    }
}