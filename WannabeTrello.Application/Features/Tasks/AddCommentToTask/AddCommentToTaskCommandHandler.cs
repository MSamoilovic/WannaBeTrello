using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.AddCommentToTask;

public class AddCommentToTaskCommandHandler(
    IBoardTaskService boardTaskService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AddCommentToTaskCommand, long>
{
    public async Task<long> Handle(AddCommentToTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }
        
        var comment =  await boardTaskService.AddCommentToTaskAsync(
            request.TaskId,
            currentUserService.UserId.Value,
            request.Content
        );

        await InvalidateCacheAsync(request.TaskId, cancellationToken);
        
        return comment;
    }

    private async Task InvalidateCacheAsync(long taskId, CancellationToken ct)
    {
        await cacheService.RemoveAsync(CacheKeys.TaskComments(taskId), ct);
    }
}