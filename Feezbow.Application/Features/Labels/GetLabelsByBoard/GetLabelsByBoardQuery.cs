using MediatR;

namespace WannabeTrello.Application.Features.Labels.GetLabelsByBoard;

public record GetLabelsByBoardQuery(long BoardId) : IRequest<IReadOnlyList<GetLabelsByBoardQueryResponse>>;
