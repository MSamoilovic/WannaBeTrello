using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.GetUserOwnedProjects;

public class GetUserOwnedProjectsQueryHandler(IUserService userService, ICurrentUserService currentUserService)
    : IRequestHandler<GetUserOwnedProjectsQuery, GetUserOwnedProjectsQueryResponse>
{
    public async Task<GetUserOwnedProjectsQueryResponse> Handle(GetUserOwnedProjectsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var ownedProjects = await userService.GetUserOwnedProjectsAsync(request.UserId, cancellationToken);

        return GetUserOwnedProjectsQueryResponse.FromEntities(ownedProjects);
    }
}

