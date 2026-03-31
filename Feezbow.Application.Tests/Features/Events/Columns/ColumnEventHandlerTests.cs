using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Columns;
using Feezbow.Domain.Events.Column_Events;

namespace Feezbow.Application.Tests.Features.Events.Columns;

public class ColumnAddedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyColumnCreated_WithCorrectParameters()
    {
        // Arrange
        var boardId = 1L;
        var columnId = 2L;
        var columnName = "To Do";
        var creatorUserId = 3L;
        var notification = new ColumnAddedEvent(boardId, columnId, columnName, creatorUserId);

        var notificationServiceMock = new Mock<IColumnNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyColumnCreated(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ColumnAddedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyColumnCreated(columnId, columnName, boardId, creatorUserId),
            Times.Once);
    }
}

public class ColumnUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyColumnUpdated_WithCorrectParameters()
    {
        // Arrange
        var columnId = 1L;
        var oldName = "Old Name";
        var newName = "New Name";
        var boardId = 5L;
        var modifierUserId = 10L;
        var notification = new ColumnUpdatedEvent(columnId, oldName, newName, boardId, modifierUserId);

        var notificationServiceMock = new Mock<IColumnNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyColumnUpdated(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ColumnUpdatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyColumnUpdated(columnId, oldName, newName, boardId, modifierUserId),
            Times.Once);
    }
}

public class ColumnDeletedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyColumnDeleted_WithCorrectParameters()
    {
        // Arrange
        var columnId = 1L;
        var boardId = 5L;
        var modifierUserId = 10L;
        var notification = new ColumnDeletedEvent(columnId, boardId, modifierUserId);

        var notificationServiceMock = new Mock<IColumnNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyColumnDeletedEvent(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ColumnDeletedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyColumnDeletedEvent(columnId, boardId, modifierUserId),
            Times.Once);
    }
}

public class ColumnOrderChangedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyColumnOrderChanged_WithCorrectParameters()
    {
        // Arrange
        var columnId = 1L;
        var boardId = 5L;
        var oldOrder = 1;
        var newOrder = 3;
        var modifierUserId = 10L;
        var notification = new ColumnOrderChangedEvent(columnId, boardId, oldOrder, newOrder, modifierUserId);

        var notificationServiceMock = new Mock<IColumnNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyColumnOrderChanged(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ColumnOrderChangedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyColumnOrderChanged(columnId, boardId, oldOrder, newOrder, modifierUserId),
            Times.Once);
    }
}

public class ColumnWipLimitChangedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyColumnWipLimitChanged_WithCorrectParameters()
    {
        // Arrange
        var columnId = 1L;
        var boardId = 5L;
        var oldWipLimit = (int?)3;
        var newWipLimit = (int?)5;
        var modifierUserId = 10L;
        var notification = new ColumnWipLimitChangedEvent(columnId, boardId, oldWipLimit, newWipLimit, modifierUserId);

        var notificationServiceMock = new Mock<IColumnNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyColumnWipLimitChanged(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ColumnWipLimitChangedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyColumnWipLimitChanged(columnId, boardId, oldWipLimit, newWipLimit, modifierUserId),
            Times.Once);
    }
}
