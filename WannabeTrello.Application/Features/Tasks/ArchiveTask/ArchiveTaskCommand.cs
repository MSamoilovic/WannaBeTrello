using MediatR;

namespace WannabeTrello.Application.Features.Tasks.ArchiveTask;

public record ArchiveTaskCommand(long TaskId): IRequest<ArchiveTaskCommandResponse>;