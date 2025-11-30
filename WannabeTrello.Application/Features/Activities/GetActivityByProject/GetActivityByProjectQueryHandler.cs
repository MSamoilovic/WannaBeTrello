using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Activities.GetActivityByProject;

public class GetActivityByProjectQueryHandler(IActivityLogService activityLogService, ICurrentUserService currentUserService) : IRequestHandler<GetActivityByProjectQuery, IReadOnlyList<GetActivityByProjectQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByProjectQueryResponse>> Handle(GetActivityByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var res = await activityLogService.GetActivitiesForProjectAsync(request.ProjectId, cancellationToken);

        return [.. res.Select(act => GetActivityByProjectQueryResponse.FromEntity(act))];
    }
}
