using MediatR;

namespace WannabeTrello.Application.Features.Tasks.RestoreTask;

public record RestoreTaskCommand(long TaskId): IRequest<RestoreTaskCommandResponse>;