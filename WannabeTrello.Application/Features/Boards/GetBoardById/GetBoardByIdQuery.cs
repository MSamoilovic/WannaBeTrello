using MediatR;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public record GetBoardByIdQuery(long BoardId): IRequest<GetBoardByIdQueryResponse>;