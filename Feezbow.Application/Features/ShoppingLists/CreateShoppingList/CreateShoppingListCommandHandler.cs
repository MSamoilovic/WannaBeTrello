using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.ShoppingLists.CreateShoppingList;

public class CreateShoppingListCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateShoppingListCommand, CreateShoppingListCommandResponse>
{
    public async Task<CreateShoppingListCommandResponse> Handle(
        CreateShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var list = ShoppingList.Create(request.ProjectId, request.Name, userId);

        await unitOfWork.ShoppingLists.AddAsync(list, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(request.ProjectId), cancellationToken);

        var result = Result<long>.Success(list.Id, "Shopping list created successfully.");
        return new CreateShoppingListCommandResponse(result);
    }
}
