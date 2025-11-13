using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserProjects;

public class GetUserProjectsQueryHandler(IUserService userService, ICurrentUserService currentUserService)
    : IRequestHandler<GetUserProjectsQuery, GetUserProjectsQueryResponse>
{
    public async Task<GetUserProjectsQueryResponse> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var projects = await userService.GetUserProjectsAsync(request.UserId, cancellationToken);

        return GetUserProjectsQueryResponse.FromEntities(projects);
    }
}

