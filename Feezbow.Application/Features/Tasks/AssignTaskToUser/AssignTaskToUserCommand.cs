using MediatR;

namespace Feezbow.Application.Features.Tasks.AssignTaskToUser;

public record AssignTaskToUserCommand(long TaskId, long newAssigneeId): IRequest<AssignTaskToUserCommandResponse>;