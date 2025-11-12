using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Users.GetUserProfile;

public class GetUserProfileQueryResponse
{
    public long Id { get; init; }
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Bio { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public DateTime CreatedAt { get; init; }

    public int PublicProjectsCount { get; init; }
    public int CompletedTasksCount { get; init; }

    public static GetUserProfileQueryResponse FromEntity(User user)
    {
        return new GetUserProfileQueryResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            PublicProjectsCount = user.ProjectMemberships.Count(),
            CompletedTasksCount = user.AssignedTasks.Count(),
        };
    }
}
