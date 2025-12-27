using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.UpdateTask;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndTaskExists_ShouldUpdateTaskAndReturnSuccessResponse()
    {
        // Arrange
        const long userId = 123L;
        const long taskId = 456L;
        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "Updated Task Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Task sa Column-om koji ima BoardId (za invalidaciju keša)
        var taskWithColumn = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Id), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.AssigneeId), 999L);
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), 123L);
        ApplicationTestUtils.SetPrivatePropertyValue(taskWithColumn, nameof(BoardTask.Column), column);
        
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskWithColumn);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(taskId, response.Result.Value);
        Assert.Equal("Task updated successfully", response.Result.Message);

        taskServiceMock.Verify(
            s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Task(taskId)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardTasks(123L)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.UserTasks(999L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            TaskId = 1L,
            Title = "Test",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
        
        taskServiceMock.Verify(
            s => s.UpdateTaskDetailsAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TaskPriority>(),
                It.IsAny<DateTime>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateTaskCommand
        {
            TaskId = 1L,
            Title = "Test",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
        
        taskServiceMock.Verify(
            s => s.UpdateTaskDetailsAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TaskPriority>(),
                It.IsAny<DateTime>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        const long userId = 123L;
        const long nonExistentTaskId = 999L;
        var command = new UpdateTaskCommand
        {
            TaskId = nonExistentTaskId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                nonExistentTaskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(BoardTask), nonExistentTaskId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Entity 'BoardTask' ({nonExistentTaskId}) was not found.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHavePermission_ShouldThrowAccessDeniedException()
    {
        // Arrange
        const long userId = 123L;
        const long taskId = 456L;
        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                taskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("You don't have a permission to update this task."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have a permission to update this task.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        const long userId = 123L;
        const long taskId = 456L;
        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "", // Invalid empty title
            Description = "Some description",
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                taskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Task title cannot be empty."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Task title cannot be empty.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        const long userId = 123L;
        const long taskId = 456L;
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.Urgent,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                cancellationToken))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        taskServiceMock.Verify(
            s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                command.Description,
                command.Priority,
                command.DueDate,
                userId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldUpdateTaskSuccessfully()
    {
        // Arrange
        const long userId = 123L;
        const long taskId = 456L;
        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Title = "Task Without Description",
            Description = null, // Null description
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                null,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateTaskCommandHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(taskId, response.Result.Value);

        taskServiceMock.Verify(
            s => s.UpdateTaskDetailsAsync(
                command.TaskId,
                command.Title,
                null,
                command.Priority,
                command.DueDate,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

