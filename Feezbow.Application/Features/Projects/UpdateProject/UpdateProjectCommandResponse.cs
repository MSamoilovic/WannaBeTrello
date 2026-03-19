using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommandResponse(
    string? Name,
    string? Description,
    ProjectVisibility Visibility,
    ProjectStatus Status,
    bool? IsArchived);
