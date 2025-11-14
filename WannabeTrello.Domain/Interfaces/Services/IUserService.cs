using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IUserService
{
    User CreateUserForAuth(string userName, string email, string? firstName = null, string? lastName = null, string? bio = null, string? profilePictureUrl = null, long? createdBy = null);
    Task<User> CreateUserAsync( string userName,
        string email,
        string? firstName = null,
        string? lastName = null,
        string? bio = null,
        string? profilePictureUrl = null,
        long? createdBy = null, CancellationToken cancellationToken = default);
    
    Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Project>> GetUserOwnedProjectsAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BoardTask>> GetUserAssignedTasksAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Project>> GetUserProjectsAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Board>> GetUserBoardMemberships(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Comment>> GetUserCommentsAsync(long userId, CancellationToken cancellationToken);
    Task UpdateUserProfileAsync(long userId, string? firstName, string? lastName, string? bio, string profilePictureUrl, long modifiedBy, CancellationToken cancellationToken);
    Task DeactivateUserAsync(long userId, long modifierUserId, CancellationToken cancellationToken);
    Task ReactivateUserAsync(long userId, long modifierUserId, CancellationToken cancellationToken);
    IQueryable<User> SearchUsers();
}