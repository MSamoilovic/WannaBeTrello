using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.AssignTaskToUser;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
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

        // Task sa Column-om koji ima BoardId (za invalidaciju keša) - pre assignmenta
        var taskBefore = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskBefore, nameof(BoardTask.Id), command.TaskId);
        ApplicationTestUtils.SetPrivatePropertyValue(taskBefore, nameof(BoardTask.AssigneeId), 500L); // old assignee
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), 123L);
        ApplicationTestUtils.SetPrivatePropertyValue(taskBefore, nameof(BoardTask.Column), column);
        
        // Task nakon assignmenta (za invalidaciju keša)
        var taskAfter = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskAfter, nameof(BoardTask.Id), command.TaskId);
        ApplicationTestUtils.SetPrivatePropertyValue(taskAfter, nameof(BoardTask.AssigneeId), command.newAssigneeId);
        ApplicationTestUtils.SetPrivatePropertyValue(taskAfter, nameof(BoardTask.Column), column);

        boardTaskServiceMock
            .SetupSequence(s => s.GetTaskByIdAsync(command.TaskId, performerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskBefore) // Pre assignmenta
            .ReturnsAsync(taskAfter); // Nakon assignmenta u InvalidateCacheAsync

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(command.TaskId, response.Result.Value);
        Assert.Equal("Task assigned to user", response.Result.Message);

        boardTaskServiceMock.Verify(
            s => s.AssignTaskToUserAsync(command.TaskId, command.newAssigneeId, performerId, It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Verify GetTaskByIdAsync is called twice - once before assignment and once after for cache invalidation
        boardTaskServiceMock.Verify(
            s => s.GetTaskByIdAsync(command.TaskId, performerId, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Task(command.TaskId)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardTasks(123L)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.UserTasks(500L)), It.IsAny<CancellationToken>()), Times.Once); // old assignee
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.UserTasks(command.newAssigneeId)), It.IsAny<CancellationToken>()), Times.Once); // new assignee
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrow()
    {
        // Arrange
        var command = new AssignTaskToUserCommand(42L, 1001L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

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
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new AssignTaskToUserCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.AssignTaskToUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

