using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Activities.GetActivityByUser;

public class GetActivityByUserQueryHandler(IActivityLogService activityLogService, ICurrentUserService currentUserService) 
    : IRequestHandler<GetActivityByUserQuery, IReadOnlyList<GetActivityByUserQueryResponse>>
{
    public async Task<IReadOnlyList<GetActivityByUserQueryResponse>> Handle(GetActivityByUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var res = await activityLogService.GetActivitiesForUserAsync(request.UserId, cancellationToken);

        return [.. res.Select(act => GetActivityByUserQueryResponse.FromEntity(act))];
    }
}

