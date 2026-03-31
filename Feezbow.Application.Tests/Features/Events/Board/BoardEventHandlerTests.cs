using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Board;
using Feezbow.Domain.Events.Board_Events;
using Feezbow.Domain.Events;

namespace Feezbow.Application.Tests.Features.Events.Board;

public class BoardCreatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyBoardCreated_WithCorrectParameters()
    {
        // Arrange
        var boardId = 1L;
        var projectId = 2L;
        var boardName = "Test Board";
        var creatorUserId = 3L;
        var notification = new BoardCreatedEvent(boardId, boardName, "description", creatorUserId, projectId);

        var notificationServiceMock = new Mock<IBoardNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyBoardCreated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new BoardCreatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyBoardCreated(boardId, projectId, boardName, creatorUserId),
            Times.Once);
    }
}

public class BoardUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyBoardUpdated_WithCorrectParameters()
    {
        // Arrange
        var boardId = 1L;
        var modifierUserId = 5L;
        var notification = new BoardUpdatedEvent(
            boardId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?>(),
            modifierUserId);

        var notificationServiceMock = new Mock<IBoardNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyBoardUpdated(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new BoardUpdatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyBoardUpdated(boardId, modifierUserId),
            Times.Once);
    }
}

public class BoardArchivedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyBoardArchived_WithCorrectParameters()
    {
        // Arrange
        var boardId = 10L;
        var modifierUserId = 20L;
        var notification = new BoardArchivedEvent(boardId, modifierUserId);

        var notificationServiceMock = new Mock<IBoardNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyBoardArchived(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new BoardArchivedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyBoardArchived(boardId, modifierUserId),
            Times.Once);
    }
}

public class BoardRestoredEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyBoardRestored_WithCorrectParameters()
    {
        // Arrange
        var boardId = 10L;
        var modifierUserId = 20L;
        var notification = new BoardRestoredEvent(boardId, modifierUserId);

        var notificationServiceMock = new Mock<IBoardNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyBoardRestored(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new BoardRestoredEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyBoardRestored(boardId, modifierUserId),
            Times.Once);
    }
}
