using MediatR;

namespace Feezbow.Application.Features.Tasks.GetRecurringTasks;

public record GetRecurringTasksQuery(long BoardId) : IRequest<IReadOnlyList<GetRecurringTasksQueryResponse>>;
