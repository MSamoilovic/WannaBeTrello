using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.GetProjectById;

public class GetProjectByIdQueryResponse
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public ProjectVisibility? Visibility { get; private set; }
    public ProjectStatus? Status { get; private set; }
    public bool? Archived { get; private set; }

    public static GetProjectByIdQueryResponse FromEntity(Project? entity)
    {
        return new GetProjectByIdQueryResponse
        {
            Name = entity?.Name,
            Description = entity?.Description,
            Visibility = entity?.Visibility,
            Status = entity?.Status,
            Archived = entity?.IsArchived
                
        };
    }
}