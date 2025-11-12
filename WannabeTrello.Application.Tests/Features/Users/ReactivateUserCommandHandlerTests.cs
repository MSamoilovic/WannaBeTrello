using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.ReactivateUser;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class ReactivateUserCommandHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ReactivateUserCommandHandler _handler;

    public ReactivateUserCommandHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new ReactivateUserCommandHandler(_userServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndValidRequest_ShouldReactivateUserSuccessfully()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
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
        Assert.Equal("User Reactivated Successfully", response.Result.Message);

        _userServiceMock.Verify(s => s.ReactivateUserAsync(
            targetUserId,
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReactivatingOwnAccount_ShouldReactivateSuccessfully()
    {
        // Arrange
        const long userId = 123L;
        
        var command = new ReactivateUserCommand(userId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
                userId,
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _userServiceMock.Verify(s => s.ReactivateUserAsync(
            userId,
            userId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 456L;
        var command = new ReactivateUserCommand(targetUserId);
        
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.ReactivateUserAsync(
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 456L;
        var command = new ReactivateUserCommand(targetUserId);
        
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.ReactivateUserAsync(
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
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
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
    public async Task Handle_WhenUserAlreadyActive_ShouldHandleIdempotently()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        // UserService handles idempotency - no exception thrown
        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
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
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
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
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenReactivatingMultipleUsers_ShouldCallServiceForEachUser()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId1 = 456L;
        const long targetUserId2 = 789L;
        
        var command1 = new ReactivateUserCommand(targetUserId1);
        var command2 = new ReactivateUserCommand(targetUserId2);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
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

        _userServiceMock.Verify(s => s.ReactivateUserAsync(
            It.IsAny<long>(),
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenReactivatingAfterDeactivation_ShouldSucceed()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 456L;
        
        var command = new ReactivateUserCommand(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        _userServiceMock
            .Setup(s => s.ReactivateUserAsync(
                targetUserId,
                currentUserId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal("User Reactivated Successfully", response.Result.Message);

        _userServiceMock.Verify(s => s.ReactivateUserAsync(
            targetUserId,
            currentUserId,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

