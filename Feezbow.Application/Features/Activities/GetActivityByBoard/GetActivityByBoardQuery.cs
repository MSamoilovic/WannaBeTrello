using MediatR;

namespace WannabeTrello.Application.Features.Activities.GetActivityByBoard;

public record GetActivityByBoardQuery(long BoardId) : IRequest<IReadOnlyList<GetActivityByBoardQueryResponse>>;
