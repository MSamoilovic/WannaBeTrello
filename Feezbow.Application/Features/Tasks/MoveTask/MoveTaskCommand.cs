using MediatR;

namespace Feezbow.Application.Features.Tasks.MoveTask;

public record MoveTaskCommand(long TaskId, long NewColumnId) : IRequest<MoveTaskCommandResponse> ;