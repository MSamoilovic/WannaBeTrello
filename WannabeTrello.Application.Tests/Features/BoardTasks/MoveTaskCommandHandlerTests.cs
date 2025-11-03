using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.MoveTask;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class MoveTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCallServiceAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var newColumnId = 789L;
        var command = new MoveTaskCommand(taskId, newColumnId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.MoveTaskAsync(taskId, newColumnId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(taskId, response.Result.Value);
        Assert.Equal("Task moved successfully", response.Result.Message);

        taskServiceMock.Verify(
            s => s.MoveTaskAsync(taskId, newColumnId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new MoveTaskCommand(1, 2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new MoveTaskCommand(1, 2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ExceptionIsPropagated()
    {
        // Arrange
        var userId = 123L;
        var command = new MoveTaskCommand(1, 2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.MoveTaskAsync(command.TaskId, command.NewColumnId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("boom", ex.Message);
    }
}


