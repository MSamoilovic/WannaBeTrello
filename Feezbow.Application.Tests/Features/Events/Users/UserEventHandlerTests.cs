using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Users;
using Feezbow.Domain.Events.UserEvents;

namespace Feezbow.Application.Tests.Features.Events.Users;

public class UserDeactivatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyUserDeactivated_WithCorrectParameters()
    {
        // Arrange
        var userId = 1L;
        var deactivatedByUserId = 2L;
        var notification = new UserDeactivatedEvent(userId, deactivatedByUserId);

        var notificationServiceMock = new Mock<IUserNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyUserDeactivated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UserDeactivatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserDeactivated(userId, deactivatedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class UserReactivatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyUserReactivated_WithCorrectParameters()
    {
        // Arrange
        var userId = 1L;
        var reactivatedByUserId = 2L;
        var notification = new UserReactivatedEvent(userId, reactivatedByUserId);

        var notificationServiceMock = new Mock<IUserNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyUserReactivated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UserReactivatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserReactivated(userId, reactivatedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class UserProfileUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyUserProfileUpdated_WithCorrectParameters()
    {
        // Arrange
        var userId = 1L;
        var modifyingUserId = 2L;
        var oldValues = new Dictionary<string, object?> { { "FirstName", "OldName" } } as IReadOnlyDictionary<string, object?>;
        var newValues = new Dictionary<string, object?> { { "FirstName", "NewName" } } as IReadOnlyDictionary<string, object?>;
        var notification = new UserProfileUpdatedEvent(userId, oldValues!, newValues!, modifyingUserId);

        var notificationServiceMock = new Mock<IUserNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyUserProfileUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UserProfileUpdatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserProfileUpdated(userId, oldValues, newValues, modifyingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
