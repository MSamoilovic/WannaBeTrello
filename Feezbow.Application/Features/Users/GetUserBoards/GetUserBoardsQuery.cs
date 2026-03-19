using MediatR;

namespace Feezbow.Application.Features.Users.GetUserBoards;

public record GetUserBoardsQuery(long UserId) : IRequest<GetUserBoardsQueryResponse>;

