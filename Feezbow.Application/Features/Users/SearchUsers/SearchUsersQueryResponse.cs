using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Users.SearchUsers;

public class SearchUsersQueryResponse
{
    public long Id { get; init; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public static SearchUsersQueryResponse FromEntity(User user)
    {
        return new SearchUsersQueryResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
