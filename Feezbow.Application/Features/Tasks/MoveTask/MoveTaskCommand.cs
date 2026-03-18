using MediatR;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

public record MoveTaskCommand(long TaskId, long NewColumnId) : IRequest<MoveTaskCommandResponse> ;