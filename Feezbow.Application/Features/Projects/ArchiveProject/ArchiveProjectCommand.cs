using MediatR;

namespace Feezbow.Application.Features.Projects.ArchiveProject;

public record ArchiveProjectCommand(long ProjectId): IRequest<ArchiveProjectCommandResponse>;