using MediatR;

namespace WannabeTrello.Application.Features.Activities.GetActivityByTask;

public record GetActivityByTaskQuery(long TaskId) : IRequest<IReadOnlyList<GetActivityByTaskQueryResponse>>;

