using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.UpdateUserProfile;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new UpdateUserProfileCommandHandler(_userServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndValidRequest_ShouldUpdateProfileSuccessfully()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe",
            Bio = "Software Developer",
            ProfilePictureUrl = "https://example.com/photo.jpg"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                "John",
                "Doe",
                "Software Developer",
                "https://example.com/photo.jpg",
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal("User Profile updated successfully", response.Result.Message);

        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            targetUserId,
            "John",
            "Doe",
            "Software Developer",
            "https://example.com/photo.jpg",
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUpdatingWithNullValues_ShouldUpdateProfileSuccessfully()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = null,
            LastName = null,
            Bio = null,
            ProfilePictureUrl = null
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                null,
                null,
                null,
                null!,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.IsSuccess);
        

        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            targetUserId,
            null,
            null,
            null,
            null!,
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UserId = 123L,
            FirstName = "John",
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UserId = 123L,
            FirstName = "John",
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 999L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User", targetUserId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("User", exception.Message);
        Assert.Contains(targetUserId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_WhenFirstNameTooLong_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        var longFirstName = new string('a', 101); // 101 characters, exceeds max of 100
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = longFirstName,
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                longFirstName,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("First name cannot exceed 100 characters"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("First name", exception.Message);
        Assert.Contains("100 characters", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenLastNameTooLong_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        var longLastName = new string('a', 301); // 301 characters, exceeds max of 300
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = longLastName
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                It.IsAny<string>(),
                longLastName,
                It.IsAny<string>(),
                It.IsAny<string>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Last name cannot exceed 100 characters"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Last name", exception.Message);
        Assert.Contains("100 characters", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenBioTooLong_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        var longBio = new string('a', 501); // 501 characters, exceeds max of 500
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe",
            Bio = longBio
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                longBio,
                It.IsAny<string>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Bio cannot exceed 500 characters"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Bio", exception.Message);
        Assert.Contains("500 characters", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenProfilePictureUrlInvalid_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        const string invalidUrl = "not-a-valid-url";
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe",
            ProfilePictureUrl = invalidUrl
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                invalidUrl,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Profile picture URL must be a valid absolute HTTP or HTTPS URL."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Profile picture URL", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIsDeactivated_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Unable to perform action for a deactivated user."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("deactivated user", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "John",
            LastName = "Doe"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenUpdatingOnlyFirstName_ShouldUpdateOnlyFirstName()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "UpdatedName",
            LastName = null,
            Bio = null,
            ProfilePictureUrl = null
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                "UpdatedName",
                null,
                null,
                null!,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            targetUserId,
            "UpdatedName",
            null,
            null,
            null!,
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUpdatingMultipleFields_ShouldUpdateAllFields()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;
        
        var command = new UpdateUserProfileCommand
        {
            UserId = targetUserId,
            FirstName = "Jane",
            LastName = "Smith",
            Bio = "Updated bio text",
            ProfilePictureUrl = "https://example.com/new-photo.jpg"
        };

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.UpdateUserProfileAsync(
                targetUserId,
                "Jane",
                "Smith",
                "Updated bio text",
                "https://example.com/new-photo.jpg",
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _userServiceMock.Verify(s => s.UpdateUserProfileAsync(
            targetUserId,
            "Jane",
            "Smith",
            "Updated bio text",
            "https://example.com/new-photo.jpg",
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

