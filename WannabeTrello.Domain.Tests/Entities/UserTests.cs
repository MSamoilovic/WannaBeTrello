using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.UserEvents;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Tests.Utils;

namespace WannabeTrello.Domain.Tests.Entities;

public class UserTests
{
    // --- Testovi za Create metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithValidArguments_ReturnsCorrectlyInitializedUser()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        const string bio = "Test bio";
        const string profilePictureUrl = "https://example.com/picture.jpg";
        const long createdBy = 123;

        // Act
        var user = User.Create(userName, email, firstName, lastName, bio, profilePictureUrl, createdBy);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userName, user.UserName);
        Assert.Equal(email, user.Email);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(bio, user.Bio);
        Assert.Equal(profilePictureUrl, user.ProfilePictureUrl);
        Assert.Equal(createdBy, user.CreatedBy);
        Assert.True(user.IsActive);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.LastModifiedAt);
        Assert.Null(user.LastModifiedBy);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithMinimalArguments_ReturnsCorrectlyInitializedUser()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";

        // Act
        var user = User.Create(userName, email);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userName, user.UserName);
        Assert.Equal(email, user.Email);
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.Bio);
        Assert.Null(user.ProfilePictureUrl);
        Assert.Null(user.CreatedBy);
        Assert.True(user.IsActive);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithInvalidCreatedBy_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";
        const long invalidCreatedBy = 0;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            User.Create(userName, email, createdBy: invalidCreatedBy));

        Assert.Equal("Actor identifier must be a positive number.", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Domain")]
    public void Create_WithNegativeCreatedBy_ThrowsBusinessRuleValidationException(long invalidCreatedBy)
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            User.Create(userName, email, createdBy: invalidCreatedBy));

        Assert.Equal("Actor identifier must be a positive number.", exception.Message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithNameExceedingMaxLength_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";
        var longFirstName = new string('a', 101); // MaxNameLength is 100

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            User.Create(userName, email, firstName: longFirstName));

        Assert.Equal("Name cannot exceed 100 characters.", exception.Message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithBioExceedingMaxLength_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";
        var longBio = new string('a', 501); // MaxBioLength is 500

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            User.Create(userName, email, bio: longBio));

        Assert.Equal("Bio cannot exceed 500 characters.", exception.Message);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/picture.jpg")]
    [InlineData("relative/path.jpg")]
    [Trait("Category", "Domain")]
    public void Create_WithInvalidProfilePictureUrl_ThrowsBusinessRuleValidationException(string invalidUrl)
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            User.Create(userName, email, profilePictureUrl: invalidUrl));

        Assert.Equal("Profile picture URL must be a valid absolute HTTP or HTTPS URL.", exception.Message);
    }

    [Theory]
    [InlineData("http://example.com/picture.jpg")]
    [InlineData("https://example.com/picture.jpg")]
    [Trait("Category", "Domain")]
    public void Create_WithValidProfilePictureUrl_SetsUrlCorrectly(string validUrl)
    {
        // Arrange
        const string userName = "testuser";
        const string email = "test@example.com";

        // Act
        var user = User.Create(userName, email, profilePictureUrl: validUrl);

        // Assert
        Assert.Equal(validUrl, user.ProfilePictureUrl);
    }

    // --- Testovi za UpdateProfile metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithValidArguments_UpdatesPropertiesAndRaisesEvent()
    {
        // Arrange
        var user = CreateTestUser(1, "olduser", "old@example.com", "Old", "Name", "Old bio", null);
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;
        const string newFirstName = "New";
        const string newLastName = "Name";
        const string newBio = "New bio";
        const string newProfilePictureUrl = "https://example.com/new.jpg";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newBio, newProfilePictureUrl, modifyingUserId);

        // Assert
        Assert.Equal(newFirstName, user.FirstName);
        Assert.Equal(newLastName, user.LastName);
        Assert.Equal(newBio, user.Bio);
        Assert.Equal(newProfilePictureUrl, user.ProfilePictureUrl);
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(modifyingUserId, user.LastModifiedBy);

        var domainEvent = Assert.Single(user.DomainEvents);
        var updatedEvent = Assert.IsType<UserProfileUpdatedEvent>(domainEvent);
        Assert.Equal(user.Id, updatedEvent.UserId);
        Assert.Equal(modifyingUserId, updatedEvent.ModifyingUserId);
        Assert.Contains("FirstName", updatedEvent.OldValues.Keys);
        Assert.Contains("FirstName", updatedEvent.NewValues.Keys);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithNoChanges_DoesNotUpdateAndDoesNotRaiseEvent()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const string bio = "Bio";
        const string profilePictureUrl = "https://example.com/picture.jpg";
        var user = CreateTestUser(1, "testuser", "test@example.com", firstName, lastName, bio, profilePictureUrl);
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;
        var initialModifiedAt = user.LastModifiedAt;
        var initialModifiedBy = user.LastModifiedBy;

        // Act
        user.UpdateProfile(firstName, lastName, bio, profilePictureUrl, modifyingUserId);

        // Assert
        Assert.Equal(initialModifiedAt, user.LastModifiedAt);
        Assert.Equal(initialModifiedBy, user.LastModifiedBy);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithPartialChanges_UpdatesOnlySpecifiedProperties()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "Old", "Name", "Old bio", null);
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;
        var initialBio = user.Bio;
        var initialProfilePictureUrl = user.ProfilePictureUrl;

        // Act - Pass current values for bio and profilePictureUrl to keep them unchanged
        user.UpdateProfile("New", "Name", initialBio, initialProfilePictureUrl, modifyingUserId);

        // Assert
        Assert.Equal("New", user.FirstName);
        Assert.Equal("Name", user.LastName);
        Assert.Equal(initialBio, user.Bio); // Bio should remain unchanged
        Assert.Equal(initialProfilePictureUrl, user.ProfilePictureUrl); // ProfilePictureUrl should remain unchanged
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(modifyingUserId, user.LastModifiedBy);
        Assert.Single(user.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithWhitespaceNames_NormalizesToNull()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "Old", "Name", null, null);
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;

        // Act
        user.UpdateProfile("   ", "   ", null, null, modifyingUserId);

        // Assert
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithNullValues_SetsPropertiesToNull()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", "Bio text", "https://example.com/pic.jpg");
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;

        // Act
        user.UpdateProfile(null, null, null, null, modifyingUserId);

        // Assert
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.Bio);
        Assert.Null(user.ProfilePictureUrl);
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(modifyingUserId, user.LastModifiedBy);
        Assert.Single(user.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_OnDeactivatedUser_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.Deactivate(123);
        InitializeDomainEvents(user);
        const long modifyingUserId = 456;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            user.UpdateProfile("New", "Name", null, null, modifyingUserId));

        Assert.Equal("Unable to perform action for a deactivated user.", exception.Message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateProfile_WithInvalidModifyingUserId_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        const long invalidModifyingUserId = 0;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            user.UpdateProfile("New", "Name", null, null, invalidModifyingUserId));

        Assert.Equal("Actor identifier must be a positive number.", exception.Message);
    }

    // --- Testovi za SetName metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void SetName_WithValidArguments_UpdatesName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "Old", "Name", "Bio", null);
        InitializeDomainEvents(user);
        const long modifyingUserId = 123;
        const string newFirstName = "New";
        const string newLastName = "Name";

        // Act
        user.SetName(newFirstName, newLastName, modifyingUserId);

        // Assert
        Assert.Equal(newFirstName, user.FirstName);
        Assert.Equal(newLastName, user.LastName);
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(modifyingUserId, user.LastModifiedBy);
    }

    // --- Testovi za UpdateLastLogin metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateLastLogin_SetsLastLoginAtToCurrentTime()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", null, null, null, null);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= beforeUpdate);
        Assert.True(user.LastLoginAt <= afterUpdate);
    }

    // --- Testovi za Deactivate metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Deactivate_ActiveUser_DeactivatesAndRaisesEvent()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        InitializeDomainEvents(user);
        const long deactivatedByUserId = 123;
        var initialSecurityStamp = user.SecurityStamp;

        // Act
        user.Deactivate(deactivatedByUserId);

        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.DeactivatedAt);
        Assert.True(user.DeactivatedAt <= DateTime.UtcNow);
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(deactivatedByUserId, user.LastModifiedBy);
        Assert.NotEqual(initialSecurityStamp, user.SecurityStamp);

        var domainEvent = Assert.Single(user.DomainEvents);
        var deactivatedEvent = Assert.IsType<UserDeactivatedEvent>(domainEvent);
        Assert.Equal(user.Id, deactivatedEvent.UserId);
        Assert.Equal(deactivatedByUserId, deactivatedEvent.DeactivatedByUserId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Deactivate_AlreadyDeactivatedUser_DoesNothing()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.Deactivate(123);
        InitializeDomainEvents(user);
        var initialDeactivatedAt = user.DeactivatedAt;
        const long deactivatedByUserId = 456;

        // Act
        user.Deactivate(deactivatedByUserId);

        // Assert
        Assert.False(user.IsActive);
        Assert.Equal(initialDeactivatedAt, user.DeactivatedAt);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Deactivate_WithInvalidDeactivatedByUserId_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        const long invalidDeactivatedByUserId = 0;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            user.Deactivate(invalidDeactivatedByUserId));

        Assert.Equal("Actor identifier must be a positive number.", exception.Message);
    }

    // --- Testovi za Reactivate metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Reactivate_DeactivatedUser_ReactivatesAndRaisesEvent()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.Deactivate(123);
        InitializeDomainEvents(user);
        const long reactivatedByUserId = 456;
        var initialSecurityStamp = user.SecurityStamp;

        // Act
        user.Reactivate(reactivatedByUserId);

        // Assert
        Assert.True(user.IsActive);
        Assert.Null(user.DeactivatedAt);
        Assert.NotNull(user.LastModifiedAt);
        Assert.Equal(reactivatedByUserId, user.LastModifiedBy);
        Assert.NotEqual(initialSecurityStamp, user.SecurityStamp);

        var domainEvent = Assert.Single(user.DomainEvents);
        var reactivatedEvent = Assert.IsType<UserReactivatedEvent>(domainEvent);
        Assert.Equal(user.Id, reactivatedEvent.UserId);
        Assert.Equal(reactivatedByUserId, reactivatedEvent.ReactivatedByUserId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Reactivate_AlreadyActiveUser_DoesNothing()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        InitializeDomainEvents(user);
        const long reactivatedByUserId = 123;

        // Act
        user.Reactivate(reactivatedByUserId);

        // Assert
        Assert.True(user.IsActive);
        Assert.Empty(user.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Reactivate_WithInvalidReactivatedByUserId_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.Deactivate(123);
        const long invalidReactivatedByUserId = 0;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            user.Reactivate(invalidReactivatedByUserId));

        Assert.Equal("Actor identifier must be a positive number.", exception.Message);
    }

    // --- Testovi za EnsureActive metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void EnsureActive_ActiveUser_DoesNotThrow()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);

        // Act & Assert
        user.EnsureActive(); // Should not throw
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void EnsureActive_DeactivatedUser_ThrowsBusinessRuleValidationException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.Deactivate(123);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => user.EnsureActive());
        Assert.Equal("Unable to perform action for a deactivated user.", exception.Message);
    }

    // --- Testovi za RequestPasswordReset metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void RequestPasswordReset_SetsPropertiesAndRaisesEvent()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        InitializeDomainEvents(user);
        const string ipAddress = "192.168.1.1";

        // Act
        user.RequestPasswordReset(ipAddress);

        // Assert
        Assert.NotNull(user.PasswordResetRequestedAt);
        Assert.True(user.PasswordResetRequestedAt <= DateTime.UtcNow);
        Assert.Equal(ipAddress, user.PasswordResetRequestIpAddress);

        var domainEvent = Assert.Single(user.DomainEvents);
        var resetRequestedEvent = Assert.IsType<PasswordResetRequestedEvent>(domainEvent);
        Assert.Equal(user.Id, resetRequestedEvent.UserId);
        Assert.Equal(user.Email, resetRequestedEvent.Email);
        Assert.Equal(ipAddress, resetRequestedEvent.IpAddress);
    }

    // --- Testovi za CompletePasswordReset metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void CompletePasswordReset_ClearsPropertiesAndRaisesEvent()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.RequestPasswordReset("192.168.1.1");
        InitializeDomainEvents(user);
        var initialSecurityStamp = user.SecurityStamp;
        const string ipAddress = "192.168.1.2";

        // Act
        user.CompletePasswordReset(ipAddress);

        // Assert
        Assert.Null(user.PasswordResetRequestedAt);
        Assert.Null(user.PasswordResetRequestIpAddress);
        Assert.NotEqual(initialSecurityStamp, user.SecurityStamp);

        var domainEvent = Assert.Single(user.DomainEvents);
        var resetCompletedEvent = Assert.IsType<PasswordResetCompletedEvent>(domainEvent);
        Assert.Equal(user.Id, resetCompletedEvent.UserId);
        Assert.Equal(user.Email, resetCompletedEvent.Email);
        Assert.Equal(ipAddress, resetCompletedEvent.IpAddress);
    }

    // --- Testovi za RequestEmailConfirmation metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void RequestEmailConfirmation_SetsProperties()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        const string ipAddress = "192.168.1.1";

        // Act
        user.RequestEmailConfirmation(ipAddress);

        // Assert
        Assert.NotNull(user.EmailConfirmationRequestedAt);
        Assert.True(user.EmailConfirmationRequestedAt <= DateTime.UtcNow);
        Assert.Equal(ipAddress, user.EmailConfirmationRequestIpAddress);
    }

    // --- Testovi za CompleteEmailConfirmation metodu ---

    [Fact]
    [Trait("Category", "Domain")]
    public void CompleteEmailConfirmation_SetsPropertiesAndClearsRequest()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);
        user.RequestEmailConfirmation("192.168.1.1");
        const string ipAddress = "192.168.1.2";

        // Act
        user.CompleteEmailConfirmation(ipAddress);

        // Assert
        Assert.NotNull(user.EmailConfirmedAt);
        Assert.True(user.EmailConfirmedAt <= DateTime.UtcNow);
        Assert.Null(user.EmailConfirmationRequestedAt);
        Assert.Null(user.EmailConfirmationRequestIpAddress);
    }

    // --- Testovi za DisplayName property ---

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithFirstNameAndLastName_ReturnsFullName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", "Doe", null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("John Doe", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithOnlyFirstName_ReturnsFirstName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "John", null, null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("John", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithOnlyLastName_ReturnsLastName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", null, "Doe", null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("Doe", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithoutName_ReturnsUserName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", null, null, null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("testuser", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithoutNameAndUserName_ReturnsEmail()
    {
        // Arrange
        var user = CreateTestUser(1, null, "test@example.com", null, null, null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("test@example.com", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithoutNameUserNameAndEmail_ReturnsUserId()
    {
        // Arrange
        var user = CreateTestUser(1, null, null, null, null, null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal($"user-{user.Id}", displayName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void DisplayName_WithWhitespaceNames_ReturnsUserName()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser", "test@example.com", "   ", "   ", null, null);

        // Act
        var displayName = user.DisplayName;

        // Assert
        Assert.Equal("testuser", displayName);
    }

    // --- Helper metode ---

    private static User CreateTestUser(
        long id,
        string? userName,
        string? email,
        string? firstName,
        string? lastName,
        string? bio,
        string? profilePictureUrl)
    {
        var user = User.Create(
            userName ?? "testuser",
            email ?? "test@example.com",
            firstName,
            lastName,
            bio,
            profilePictureUrl);

        DomainTestUtils.SetPrivatePropertyValue(user, "Id", id);
        if (userName == null)
        {
            DomainTestUtils.SetPrivatePropertyValue(user, "UserName", null);
        }
        if (email == null)
        {
            DomainTestUtils.SetPrivatePropertyValue(user, "Email", null);
        }

        InitializeDomainEvents(user);
        return user;
    }

    private static void InitializeDomainEvents(User user)
    {
        var domainEventsList = new List<DomainEvent>();
        DomainTestUtils.SetPrivatePropertyValue(user, "_domainEvents", domainEventsList);
    }
}

