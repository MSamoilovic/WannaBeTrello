using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommand(
    long ProjectId,
    string? Name,
    string? Description,
    ProjectStatus Status,
    ProjectVisibility Visibility,
    bool Archived
    ) : IRequest<UpdateProjectCommandResponse>;
