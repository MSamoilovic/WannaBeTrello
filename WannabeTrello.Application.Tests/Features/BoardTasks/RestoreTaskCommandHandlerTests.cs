using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.RestoreTask;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class RestoreTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldRestoreTask()
    {
        // Arrange
        var command = new RestoreTaskCommand(321L);
        var userId = 11L;

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();

        var handler = new RestoreTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(command.TaskId, response.Result.Value);
        Assert.Equal("Restored task", response.Result.Message);

        boardTaskServiceMock.Verify(
            s => s.RestoreTaskAsync(command.TaskId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrow()
    {
        // Arrange
        var command = new RestoreTaskCommand(321L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var handler = new RestoreTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.RestoreTaskAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrow()
    {
        // Arrange
        var command = new RestoreTaskCommand(321L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var handler = new RestoreTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.RestoreTaskAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

