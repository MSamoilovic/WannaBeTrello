using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.BoardTask;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;
using CommentEntity = Feezbow.Domain.Entities.Comment;

namespace Feezbow.Application.Tests.Features.Events.BoardTask;

public class TaskCreatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyTaskCreated_WithCorrectParameters()
    {
        // Arrange
        var taskId = 1L;
        var boardId = 10L;
        var taskTitle = "New Task";
        var creatorUserId = 2L;
        var assigneeId = (long?)3L;
        var notification = new TaskCreatedEvent(taskId, taskTitle, creatorUserId, assigneeId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyTaskCreated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long?>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new TaskCreatedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskCreated(taskId, boardId, taskTitle, creatorUserId, assigneeId),
            Times.Once);
    }
}

public class TaskUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyTaskUpdated_WithCorrectParameters()
    {
        // Arrange
        var taskId = 1L;
        var boardId = 10L;
        var taskName = "Updated Task";
        var modifierUserId = 5L;
        var oldValues = new Dictionary<string, object?> { { "Title", "Old Title" } };
        var newValues = new Dictionary<string, object?> { { "Title", "New Title" } };
        var notification = new TaskUpdatedEvent(taskId, taskName, modifierUserId, oldValues, newValues);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyTaskUpdated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<Dictionary<string, object?>>(), It.IsAny<Dictionary<string, object?>>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new TaskUpdatedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskUpdated(taskId, boardId, taskName, modifierUserId, oldValues, newValues),
            Times.Once);
    }
}

public class TaskAssignedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyTaskAssigned_WithCorrectParameters()
    {
        // Arrange
        var taskId = 1L;
        var boardId = 10L;
        var oldAssigneeId = (long?)2L;
        var newAssigneeId = (long?)3L;
        var assignedByUserId = 5L;
        var notification = new TaskAssignedEvent(taskId, oldAssigneeId, newAssigneeId, assignedByUserId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyTaskAssigned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new TaskAssignedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskAssigned(taskId, boardId, oldAssigneeId, newAssigneeId, assignedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class TaskMovedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyTaskMoved_WithCorrectParameters()
    {
        // Arrange
        var taskId = 1L;
        var boardId = 10L;
        var originalColumnId = 20L;
        var newColumnId = 30L;
        var performedByUserId = 5L;
        var notification = new TaskMovedEvent(taskId, originalColumnId, newColumnId, performedByUserId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyTaskMoved(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new TaskMovedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskMoved(taskId, boardId, newColumnId, performedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class TaskCommentedEventHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommentFound_ShouldCallNotifyTaskCommented_WithCorrectParameters()
    {
        // Arrange
        var taskId = 1L;
        var commentId = 2L;
        var commentAuthorId = 3L;
        var boardId = 10L;
        var commentContent = "This is a comment";
        var notification = new TaskCommentedEvent(taskId, commentId, commentAuthorId, boardId);

        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<CommentEntity>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, "Content", commentContent);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyTaskCommented(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var commentRepositoryMock = new Mock<ICommentRepository>();
        commentRepositoryMock
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var handler = new TaskCommentedEventHandler(taskNotificationServiceMock.Object, commentRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskCommented(taskId, boardId, commentId, commentAuthorId, commentContent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCommentIsNull_ShouldNotCallNotifyTaskCommented()
    {
        // Arrange
        var taskId = 1L;
        var commentId = 2L;
        var commentAuthorId = 3L;
        var boardId = 10L;
        var notification = new TaskCommentedEvent(taskId, commentId, commentAuthorId, boardId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();

        var commentRepositoryMock = new Mock<ICommentRepository>();
        commentRepositoryMock
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommentEntity?)null);

        var handler = new TaskCommentedEventHandler(taskNotificationServiceMock.Object, commentRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyTaskCommented(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
