using MediatR;

namespace Feezbow.Application.Features.Labels.GetLabelsByBoard;

public record GetLabelsByBoardQuery(long BoardId) : IRequest<IReadOnlyList<GetLabelsByBoardQueryResponse>>;
