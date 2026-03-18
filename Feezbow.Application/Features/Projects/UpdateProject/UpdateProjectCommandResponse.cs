using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommandResponse(
    string? Name,
    string? Description,
    ProjectVisibility Visibility,
    ProjectStatus Status,
    bool? IsArchived);
