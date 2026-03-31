using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Comment;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Events.Comment;

public class CommentDeletedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyCommentDeleted_WithCorrectParameters()
    {
        // Arrange
        var commentId = 1L;
        var taskId = 2L;
        var modifierUserId = 3L;
        var boardId = 10L;
        var notification = new CommentDeletedEvent(commentId, taskId, modifierUserId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyCommentDeleted(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new CommentDeletedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyCommentDeleted(taskId, boardId, commentId, modifierUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class CommentUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyCommentUpdated_WithCorrectParameters()
    {
        // Arrange
        var commentId = 1L;
        var taskId = 2L;
        var modifyingUserId = 3L;
        var boardId = 10L;
        var oldContent = new Dictionary<string, object?> { { "Content", "Old content" } };
        var newContent = new Dictionary<string, object?> { { "Content", "New content" } };
        var notification = new CommentUpdatedEvent(commentId, taskId, oldContent, newContent, modifyingUserId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyCommentUpdated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Dictionary<string, object?>>(), It.IsAny<Dictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new CommentUpdatedEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyCommentUpdated(taskId, boardId, commentId, modifyingUserId, oldContent, newContent, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class CommentRestoredEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyCommentRestored_WithCorrectParameters()
    {
        // Arrange
        var commentId = 1L;
        var taskId = 2L;
        var modifyingUserId = 3L;
        var boardId = 10L;
        var notification = new CommentRestoredEvent(commentId, taskId, modifyingUserId);

        var taskNotificationServiceMock = new Mock<ITaskNotificationService>();
        taskNotificationServiceMock
            .Setup(s => s.NotifyCommentRestored(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var boardTaskRepositoryMock = new Mock<IBoardTaskRepository>();
        boardTaskRepositoryMock
            .Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var handler = new CommentRestoredEventHandler(taskNotificationServiceMock.Object, boardTaskRepositoryMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        taskNotificationServiceMock.Verify(
            s => s.NotifyCommentRestored(taskId, boardId, commentId, modifyingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
