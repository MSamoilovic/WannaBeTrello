using MediatR;

namespace Feezbow.Application.Features.Chores.AssignChore;

public record AssignChoreCommand(long ChoreId, long? AssignedToUserId) : IRequest<AssignChoreCommandResponse>;
