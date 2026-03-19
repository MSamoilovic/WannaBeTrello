using MediatR;

namespace Feezbow.Application.Features.Activities.GetActivityByBoard;

public record GetActivityByBoardQuery(long BoardId) : IRequest<IReadOnlyList<GetActivityByBoardQueryResponse>>;
