using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.CreateTask;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCreateTaskAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var newTaskId = 789L;
        var columnId = 1L;
        var boardId = 456L;
        var assigneeId = 10L;
        var command = new CreateTaskCommand
        {
            ColumnId = columnId,
            Title = "New Task Title",
            Description = "Task Description",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1),
            Position = 1,
            AssigneeId = assigneeId
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var createdTask = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(createdTask, nameof(BoardTask.Id), newTaskId);

        // Task sa Column-om koji ima BoardId (za invalidaciju keša)
        var taskWithColumn = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Id), newTaskId);
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.Id), columnId);
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), boardId);
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Column), column);

        taskServiceMock
            .Setup(s => s.CreateTaskAsync(
                command.ColumnId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                command.Position,
                command.AssigneeId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);
        
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(newTaskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskWithColumn);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new CreateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(newTaskId, response.Result.Value);
        Assert.Equal("Task created successfully", response.Result.Message);

        taskServiceMock.Verify(
            s => s.CreateTaskAsync(command.ColumnId, command.Title, command.Description, command.Priority,
                command.DueDate, command.Position, command.AssigneeId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
        
        cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.BoardTasks(boardId), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.UserTasks(assigneeId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateTaskCommand();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new CreateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Korisnik nije autentifikovan.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateTaskCommand();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new CreateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Korisnik nije autentifikovan.", exception.Message);
    }
}