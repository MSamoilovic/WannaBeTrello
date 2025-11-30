using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Activities.GetActivityByBoard;

public class GetActivityByBoardQueryHandler(IActivityLogService activityLogService, ICurrentUserService currentUserService) : IRequestHandler<GetActivityByBoardQuery, IReadOnlyList<GetActivityByBoardQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByBoardQueryResponse>> Handle(GetActivityByBoardQuery request, CancellationToken cancellationToken)
    {
        if(!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var res = await activityLogService.GetActivitiesForBoardAsync(request.BoardId, cancellationToken);

        return [.. res.Select(act => GetActivityByBoardQueryResponse.FromEntity(act))];
    }
}
