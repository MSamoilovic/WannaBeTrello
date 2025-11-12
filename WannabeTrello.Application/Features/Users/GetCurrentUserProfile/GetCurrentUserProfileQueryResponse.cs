using System.Collections.Immutable;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Users.GetCurrentUserProfile;

public class GetCurrentUserProfileQueryResponse
{
    public long Id { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }              
    public string? PhoneNumber { get; init; }       
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Bio { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<OwnedProjects> CreatedProjects { get; init; } = [];
    public IReadOnlyList<TasksAssigned> TasksAssigned { get; init; } = [];
    public IReadOnlyList<ProjectMemberships> ProjectMemberships { get; init; } = [];

    public static GetCurrentUserProfileQueryResponse FromEntity(User user)
    {
        return new GetCurrentUserProfileQueryResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            CreatedProjects = [.. user.OwnedProjects.Take(3).Select(x => new OwnedProjects(x.Id, x.Name))],
            TasksAssigned = [.. user.AssignedTasks.Take(5).Select(x => new TasksAssigned(x.Id, x.Description))],
            ProjectMemberships = [.. user.ProjectMemberships.Select(x => new ProjectMemberships(x.Project.Id, x.Project.Name))]
        };
    }

    
}

public record OwnedProjects(long ProjectId, string? ProjectName);
public record TasksAssigned(long TaskId, string? TaskName);
public record ProjectMemberships(long ProjectId, string? ProjectName);
