using MediatR;

namespace Feezbow.Application.Features.Activities.GetActivityByTask;

public record GetActivityByTaskQuery(long TaskId) : IRequest<IReadOnlyList<GetActivityByTaskQueryResponse>>;

