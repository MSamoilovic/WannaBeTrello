using Microsoft.AspNetCore.Identity;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.UserEvents;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;
public class User : IdentityUser<long>
{
    private const int MaxNameLength = 100;
    private const int MaxBioLength = 500;

    private readonly List<Project> _ownedProjects = [];
    private readonly List<ProjectMember> _projectMemberships = [];
    private readonly List<BoardMember> _boardMemberships = [];
    private readonly List<BoardTask> _assignedTasks = [];
    private readonly List<Comment> _comments = [];

    private readonly List<DomainEvent> _domainEvents = [];

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Bio { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    public string DisplayName
    {
        get
        {
            var fullName = string.Join(" ", new[] { FirstName, LastName }
                .Where(static value => !string.IsNullOrWhiteSpace(value))).Trim();

            var fallbackIdentifier = !string.IsNullOrWhiteSpace(UserName)
                ? UserName!
                : (!string.IsNullOrWhiteSpace(Email) ? Email! : $"user-{Id}");

            return string.IsNullOrWhiteSpace(fullName)
                ? fallbackIdentifier
                : fullName;
        }
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public ICollection<Project> OwnedProjects => _ownedProjects;
    public ICollection<ProjectMember> ProjectMemberships => _projectMemberships;
    public ICollection<BoardMember> BoardMemberships => _boardMemberships;
    public ICollection<BoardTask> AssignedTasks => _assignedTasks;
    public ICollection<Comment> Comments => _comments;

    public static User Create(
        string userName,
        string email,
        string? firstName = null,
        string? lastName = null,
        string? bio = null,
        string? profilePictureUrl = null,
        long? createdBy = null)
    {
        if (createdBy.HasValue)
        {
            EnsureValidActor(createdBy.Value);
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            CreatedBy = createdBy
        };

        user.UpdateProfileInternal(firstName, lastName, bio, profilePictureUrl, createdBy, suppressDomainEvent: true);
        return user;
    }

    public void UpdateProfile(string? firstName, string? lastName, string? bio, string? profilePictureUrl, long modifyingUserId)
    {
        EnsureValidActor(modifyingUserId);
        EnsureActive();
        var hasChanges = UpdateProfileInternal(firstName, lastName, bio, profilePictureUrl, modifyingUserId, suppressDomainEvent: false);

        if (!hasChanges)
        {
            return;
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifyingUserId;
    }

    public void SetName(string? firstName, string? lastName, long modifyingUserId)
    {
        UpdateProfile(firstName, lastName, Bio, ProfilePictureUrl, modifyingUserId);
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate(long deactivatedByUserId)
    {
        EnsureValidActor(deactivatedByUserId);
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = deactivatedByUserId;
        SecurityStamp = Guid.NewGuid().ToString();

        AddDomainEvent(new UserDeactivatedEvent(Id, deactivatedByUserId));
    }

    public void Reactivate(long reactivatedByUserId)
    {
        EnsureValidActor(reactivatedByUserId);
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        DeactivatedAt = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = reactivatedByUserId;
        SecurityStamp = Guid.NewGuid().ToString();

        AddDomainEvent(new UserReactivatedEvent(Id, reactivatedByUserId));
    }
    
    private bool UpdateProfileInternal(
        string? firstName,
        string? lastName,
        string? bio,
        string? profilePictureUrl,
        long? modifyingUserId,
        bool suppressDomainEvent)
    {
        var normalizedFirstName = NormalizeName(firstName);
        var normalizedLastName = NormalizeName(lastName);
        var normalizedBio = NormalizeBio(bio);
        var normalizedProfilePictureUrl = NormalizeProfilePictureUrl(profilePictureUrl);

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var hasChanges = false;

        if (!string.Equals(FirstName, normalizedFirstName, StringComparison.Ordinal))
        {
            oldValues[nameof(FirstName)] = FirstName;
            FirstName = normalizedFirstName;
            newValues[nameof(FirstName)] = FirstName;
            hasChanges = true;
        }

        if (!string.Equals(LastName, normalizedLastName, StringComparison.Ordinal))
        {
            oldValues[nameof(LastName)] = LastName;
            LastName = normalizedLastName;
            newValues[nameof(LastName)] = LastName;
            hasChanges = true;
        }

        if (!string.Equals(Bio, normalizedBio, StringComparison.Ordinal))
        {
            oldValues[nameof(Bio)] = Bio;
            Bio = normalizedBio;
            newValues[nameof(Bio)] = Bio;
            hasChanges = true;
        }

        if (!string.Equals(ProfilePictureUrl, normalizedProfilePictureUrl, StringComparison.Ordinal))
        {
            oldValues[nameof(ProfilePictureUrl)] = ProfilePictureUrl;
            ProfilePictureUrl = normalizedProfilePictureUrl;
            newValues[nameof(ProfilePictureUrl)] = ProfilePictureUrl;
            hasChanges = true;
        }

        if (hasChanges && !suppressDomainEvent && modifyingUserId.HasValue)
        {
            AddDomainEvent(new UserProfileUpdatedEvent(Id, oldValues, newValues, modifyingUserId.Value));
        }

        return hasChanges;
    }

    private static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        return trimmed.Length > MaxNameLength ? throw new BusinessRuleValidationException($"Name cannot exceed {MaxNameLength} characters.") : trimmed;
    }

    private static string? NormalizeBio(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxBioLength)
        {
            throw new BusinessRuleValidationException($"Bio cannot exceed {MaxBioLength} characters.");
        }

        return trimmed;
    }

    private static string? NormalizeProfilePictureUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleValidationException("Profile picture URL must be a valid absolute HTTP or HTTPS URL.");
        }

        return trimmed;
    }

    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw new BusinessRuleValidationException("Unable to perform action for a deactivated user.");
        }
    }

    private static void EnsureValidActor(long actorId)
    {
        if (actorId <= 0)
        {
            throw new BusinessRuleValidationException("Actor identifier must be a positive number.");
        }
    }

    private void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}