using MediatR;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommand(
    string? Name,
    string? Description,
    ProjectStatus Status,
    ProjectVisibility Visibility,
    bool Archived
    ) : IRequest<UpdateProjectCommandResponse>;
