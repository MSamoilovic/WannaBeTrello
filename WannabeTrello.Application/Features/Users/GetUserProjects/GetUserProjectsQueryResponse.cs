using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Users.GetUserProjects;

public class GetUserProjectsQueryResponse
{
    public IReadOnlyList<UserProjectDto> Projects { get; init; } = new List<UserProjectDto>();

    public static GetUserProjectsQueryResponse FromEntities(IReadOnlyList<Project> projects)
    {
        return new GetUserProjectsQueryResponse
        {
            Projects = projects.Select(UserProjectDto.FromEntity).ToList()
        };
    }
}

public class UserProjectDto
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public ProjectVisibility Visibility { get; init; }
    public ProjectStatus Status { get; init; }
    public bool IsArchived { get; init; }
    public long OwnerId { get; init; }
    public string? OwnerName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public int MembersCount { get; init; }
    public int BoardsCount { get; init; }
    public ProjectRole? UserRole { get; init; }

    public static UserProjectDto FromEntity(Project project)
    {
        return new UserProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Visibility = project.Visibility,
            Status = project.Status,
            IsArchived = project.IsArchived,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner?.DisplayName,
            CreatedAt = project.CreatedAt,
            LastModifiedAt = project.LastModifiedAt,
            MembersCount = project.ProjectMembers?.Count ?? 0,
            BoardsCount = project.Boards?.Count ?? 0,
            UserRole = project.ProjectMembers?.FirstOrDefault()?.Role
        };
    }
}

