using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.GetUserBoards;

public class GetUserBoardsQueryHandler(IUserService userService, ICurrentUserService currentUserService)
    : IRequestHandler<GetUserBoardsQuery, GetUserBoardsQueryResponse>
{
    public async Task<GetUserBoardsQueryResponse> Handle(GetUserBoardsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var boards = await userService.GetUserBoardMemberships(request.UserId, cancellationToken);

        return GetUserBoardsQueryResponse.FromEntities(boards);
    }
}

