using MediatR;

namespace WannabeTrello.Application.Features.Users.GetUserBoards;

public record GetUserBoardsQuery(long UserId) : IRequest<GetUserBoardsQueryResponse>;

