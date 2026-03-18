using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.MoveTask;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
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

        // Task sa Column-om koji ima BoardId (za invalidaciju keša)
        var taskWithColumn = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Id), taskId);
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), 123L);
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Column), column);
        
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskWithColumn);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

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
        
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Task(taskId)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardTasks(123L)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardColumns(123L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new MoveTaskCommand(1, 2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

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
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

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

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new MoveTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("boom", ex.Message);
    }
}


