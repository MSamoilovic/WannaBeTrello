using MediatR;

namespace WannabeTrello.Application.Features.Users.GetUserAssignedTasks;

public record GetUserAssignedTasksQuery(long UserId) : IRequest<GetUserAssignedTasksQueryResponse>;

