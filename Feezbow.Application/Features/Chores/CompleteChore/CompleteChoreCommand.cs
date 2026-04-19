using MediatR;

namespace Feezbow.Application.Features.Chores.CompleteChore;

public record CompleteChoreCommand(long ChoreId) : IRequest<CompleteChoreCommandResponse>;
