using MediatR;

namespace Feezbow.Application.Features.Boards.GetColumnsByBoardId;

public record GetColumnsByBoardIdQuery(long BoardId): IRequest<List<GetColumnsByBoardIdQueryResponse>>;