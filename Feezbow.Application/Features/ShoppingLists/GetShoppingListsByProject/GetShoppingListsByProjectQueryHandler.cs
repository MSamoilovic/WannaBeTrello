using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.GetShoppingListsByProject;

public class GetShoppingListsByProjectQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetShoppingListsByProjectQuery, IReadOnlyList<GetShoppingListsByProjectQueryResponse>>
{
    public async Task<IReadOnlyList<GetShoppingListsByProjectQueryResponse>> Handle(
        GetShoppingListsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (request.IncludeArchived)
        {
            var lists = await unitOfWork.ShoppingLists.GetByProjectAsync(request.ProjectId, true, cancellationToken);
            return lists.Select(GetShoppingListsByProjectQueryResponse.FromEntity).ToList();
        }

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectShoppingLists(request.ProjectId),
            async () =>
            {
                var lists = await unitOfWork.ShoppingLists.GetByProjectAsync(request.ProjectId, false, cancellationToken);
                return lists.Select(GetShoppingListsByProjectQueryResponse.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<GetShoppingListsByProjectQueryResponse>();
    }
}
