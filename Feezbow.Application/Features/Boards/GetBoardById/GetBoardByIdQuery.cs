using MediatR;

namespace Feezbow.Application.Features.Boards.GetBoardById;

public record GetBoardByIdQuery(long BoardId): IRequest<GetBoardByIdQueryResponse>;