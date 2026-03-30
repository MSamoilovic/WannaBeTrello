using MediatR;

namespace Feezbow.Application.Features.Projects.UnarchiveProject;

public record UnarchiveProjectCommand(long ProjectId) : IRequest<UnarchiveProjectCommandResponse>;
