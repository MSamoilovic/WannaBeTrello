using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.AssignTaskToUser;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class AssignTaskToUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldAssignTask()
    {
        // Arrange
        var command = new AssignTaskToUserCommand(42L, 1001L);
        var performerId = 7L;

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(performerId);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();

        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(command.TaskId, response.Result.Value);
        Assert.Equal("Task assigned to user", response.Result.Message);

        boardTaskServiceMock.Verify(
            s => s.AssignTaskToUserAsync(command.TaskId, command.newAssigneeId, performerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrow()
    {
        // Arrange
        var command = new AssignTaskToUserCommand(42L, 1001L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.AssignTaskToUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrow()
    {
        // Arrange
        var command = new AssignTaskToUserCommand(42L, 1001L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.AssignTaskToUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

