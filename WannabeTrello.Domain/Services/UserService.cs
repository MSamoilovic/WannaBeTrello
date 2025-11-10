using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class UserService : IUserService
{
    public Task<User> CreateUserAsync(string userName, string email, string? firstName = null, string? lastName = null, string? bio = null, string? profilePictureUrl = null, long? createdBy = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeactivateUserAsync(long userId, long modifierUserId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<BoardTask>> GetUserAssignedTasksAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Board>> GetUserBoardMemberships(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Comment>> GetUserCommentsAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Project>> GetUserOwnedProjectsAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Project>> GetUserProjectsAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReactivateUserAsync(long userId, long modifierUserId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<User>> SearchUsersAsync(string searchTerm, int pageSize, int pageNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User> UpdateUserProfileAsync(long userId, string? firstName, string? lastName, string? bio, string profilePictureUrl, long modifiedBy, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}