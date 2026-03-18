using MediatR;

namespace WannabeTrello.Application.Features.Projects.ArchiveProject;

public record ArchiveProjectCommand(long ProjectId): IRequest<ArchiveProjectCommandResponse>;