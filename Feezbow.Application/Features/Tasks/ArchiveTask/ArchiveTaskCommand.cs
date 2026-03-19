using MediatR;

namespace Feezbow.Application.Features.Tasks.ArchiveTask;

public record ArchiveTaskCommand(long TaskId): IRequest<ArchiveTaskCommandResponse>;