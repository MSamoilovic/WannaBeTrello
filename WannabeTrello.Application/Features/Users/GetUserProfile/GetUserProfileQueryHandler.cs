using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserProfile;

public class GetUserProfileQueryHandler(IUserService userService, ICurrentUserService currentUserService) : IRequestHandler<GetUserProfileQuery, GetUserProfileQueryResponse>
{
    public async Task<GetUserProfileQueryResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue) 
        { 
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;
        var user = await userService.GetUserProfileAsync(userId, cancellationToken);

        return GetUserProfileQueryResponse.FromEntity(user!);
    }
}
