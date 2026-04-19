using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Chores.GetChoresByProject;

public class GetChoresByProjectQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetChoresByProjectQuery, IReadOnlyList<ChoreDto>>
{
    public async Task<IReadOnlyList<ChoreDto>> Handle(
        GetChoresByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (request.IncludeCompleted)
        {
            var all = await unitOfWork.Chores.GetByProjectAsync(request.ProjectId, true, cancellationToken);
            return all.Select(ChoreDto.FromEntity).ToList();
        }

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectChores(request.ProjectId),
            async () =>
            {
                var chores = await unitOfWork.Chores.GetByProjectAsync(request.ProjectId, false, cancellationToken);
                return chores.Select(ChoreDto.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<ChoreDto>();
    }
}
