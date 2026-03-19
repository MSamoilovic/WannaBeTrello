using MediatR;

namespace Feezbow.Application.Features.Boards.GetColumnsByBoardIId;

public record GetColumnsByBoardIdQuery(long BoardId): IRequest<List<GetColumnsByBoardIdQueryResponse>>;