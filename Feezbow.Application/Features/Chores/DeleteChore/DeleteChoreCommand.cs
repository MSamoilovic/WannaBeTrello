using MediatR;

namespace Feezbow.Application.Features.Chores.DeleteChore;

public record DeleteChoreCommand(long ChoreId) : IRequest<DeleteChoreCommandResponse>;
