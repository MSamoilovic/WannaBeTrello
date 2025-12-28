using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.DeactivateUser;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeactivateUserCommandHandler _handler;

    public DeactivateUserCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new DeactivateUserCommandHandler(_userServiceMock.Object, _currentUserServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndValidRequest_ShouldDeactivateUserSuccessfully()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new DeactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                targetUserId,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal("User Deactivated Successfully", response.Result.Message);

        _userServiceMock.Verify(s => s.DeactivateUserAsync(
            targetUserId,
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.UserProfile(targetUserId)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeactivatingOwnAccount_ShouldDeactivateSuccessfully()
    {
        // Arrange
        const long userId = 123L;
        
        var command = new DeactivateUserCommand(userId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                userId,
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _userServiceMock.Verify(s => s.DeactivateUserAsync(
            userId,
            userId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 456L;
        var command = new DeactivateUserCommand(targetUserId);
        
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.DeactivateUserAsync(
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 456L;
        var command = new DeactivateUserCommand(targetUserId);
        
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.DeactivateUserAsync(
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 999L;
        
        var command = new DeactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                targetUserId,
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
    public async Task Handle_WhenUserAlreadyDeactivated_ShouldHandleIdempotently()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new DeactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        // UserService handles idempotency - no exception thrown
        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                targetUserId,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenInvalidActorId_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long currentUserId = 0L; // Invalid actor ID
        const long targetUserId = 456L;
        
        var command = new DeactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                targetUserId,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Actor identifier must be a positive number."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Actor identifier", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new DeactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenDeactivatingMultipleUsers_ShouldCallServiceForEachUser()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId1 = 456L;
        const long targetUserId2 = 789L;
        
        var command1 = new DeactivateUserCommand(targetUserId1);
        var command2 = new DeactivateUserCommand(targetUserId2);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.DeactivateUserAsync(
                It.IsAny<long>(),
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response1 = await _handler.Handle(command1, CancellationToken.None);
        var response2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.NotNull(response1);
        Assert.NotNull(response2);
        Assert.True(response1.Result.IsSuccess);
        Assert.True(response2.Result.IsSuccess);

        _userServiceMock.Verify(s => s.DeactivateUserAsync(
            It.IsAny<long>(),
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}

