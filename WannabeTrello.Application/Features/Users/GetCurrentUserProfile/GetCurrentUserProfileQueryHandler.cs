using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryHandler(IUserService userService, ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserProfileQuery, GetCurrentUserProfileQueryResponse>
{
    public async Task<GetCurrentUserProfileQueryResponse> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        if(!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        var user = await userService.GetUserProfileAsync(userId, cancellationToken);

        return GetCurrentUserProfileQueryResponse.FromEntity(user!);
    }
}
