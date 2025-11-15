using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserAssignedTasks;

public class GetUserAssignedTasksQueryHandler(IUserService userService, ICurrentUserService currentUserService)
    : IRequestHandler<GetUserAssignedTasksQuery, GetUserAssignedTasksQueryResponse>
{
    public async Task<GetUserAssignedTasksQueryResponse> Handle(GetUserAssignedTasksQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var tasks = await userService.GetUserAssignedTasksAsync(request.UserId, cancellationToken);

        return GetUserAssignedTasksQueryResponse.FromEntities(tasks);
    }
}

