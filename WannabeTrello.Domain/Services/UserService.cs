using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<User> CreateUserAsync(string userName, string email, string? firstName = null, string? lastName = null, string? bio = null, string? profilePictureUrl = null, long? createdBy = null, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException(nameof(userName));

        if(string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        var user = User.Create(userName, email, firstName, lastName, bio, profilePictureUrl, createdBy);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return user;

    }

    public User CreateUserForAuth(string userName, string email, string? firstName = null, string? lastName = null, string? bio = null, string? profilePictureUrl = null, long? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException(nameof(userName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

       return User.Create(userName, email, firstName, lastName, bio, profilePictureUrl, createdBy);
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

    public async Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken)
    {
        return await userRepository.GetUserProfileDetailsAsync(userId, cancellationToken);
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

    public async Task UpdateUserProfileAsync(long userId, string? firstName, string? lastName, string? bio, string profilePictureUrl, long modifiedBy, CancellationToken cancellationToken)
    {
       var user = await userRepository.GetUserProfileAsync(userId, cancellationToken) ?? throw new NotFoundException(nameof(User), userId);

        if (!string.IsNullOrWhiteSpace(firstName) && firstName.Length > 100)
            throw new BusinessRuleValidationException("First name cannot exceed 100 characters");

        if (!string.IsNullOrWhiteSpace(lastName) && lastName.Length > 300)
            throw new BusinessRuleValidationException("Last name cannot exceed 100 characters");

        if (!string.IsNullOrWhiteSpace(bio) && bio.Length > 500)
            throw new BusinessRuleValidationException("Bio cannot exceed 500 characters");

        //TODO: Add validation for profile picture

        user.UpdateProfile(firstName, lastName, bio, profilePictureUrl, modifiedBy);
        await unitOfWork.CompleteAsync(cancellationToken);

    }
}