using MediatR;

namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public record GetColumnsByBoardIdQuery(long BoardId): IRequest<List<GetColumnsByBoardIdQueryResponse>>;