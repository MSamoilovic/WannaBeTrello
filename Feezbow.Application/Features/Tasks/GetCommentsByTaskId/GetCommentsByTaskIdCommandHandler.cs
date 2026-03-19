using System.Collections.Immutable;
using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Tasks.GetCommentsByTaskId;

public class GetCommentsByTaskIdCommandHandler(
    ICommentService commentService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetCommentsByTaskIdCommand, IReadOnlyList<GetCommentsByTaskIdCommandResponse>>
{
    public async Task<IReadOnlyList<GetCommentsByTaskIdCommandResponse>> Handle(GetCommentsByTaskIdCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var cacheKey = CacheKeys.TaskComments(request.TaskId);
        
        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            () => commentService.GetCommentsByTaskId(request.TaskId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken
        );

        if (result == null)
        {
            return new List<GetCommentsByTaskIdCommandResponse>().AsReadOnly();
        }

        return result.Select(GetCommentsByTaskIdCommandResponse.FromEntity).ToImmutableList();
    }
}