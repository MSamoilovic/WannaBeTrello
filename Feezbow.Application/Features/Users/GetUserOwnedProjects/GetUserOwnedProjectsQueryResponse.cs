using WannabeTrello.Application.Features.Users.GetUserProjects;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Users.GetUserOwnedProjects;

public class GetUserOwnedProjectsQueryResponse
{
    public IReadOnlyList<UserProjectDto> OwnedProjects { get; init; } = new List<UserProjectDto>();

    public static GetUserOwnedProjectsQueryResponse FromEntities(IReadOnlyList<Project> projects)
    {
        return new GetUserOwnedProjectsQueryResponse
        {
            OwnedProjects = projects.Select(UserProjectDto.FromEntity).ToList()
        };
    }
}

