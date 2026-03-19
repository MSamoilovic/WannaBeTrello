using MediatR;

namespace Feezbow.Application.Features.Tasks.RestoreTask;

public record RestoreTaskCommand(long TaskId): IRequest<RestoreTaskCommandResponse>;