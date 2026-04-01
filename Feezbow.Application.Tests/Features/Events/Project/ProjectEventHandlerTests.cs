using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Project;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Project_Events;

namespace Feezbow.Application.Tests.Features.Events.Project;

public class ProjectCreatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectCreated_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var projectName = "Test Project";
        var ownerId = 2L;
        var notification = new ProjectCreatedEvent(projectId, projectName, ownerId, "A description");

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectCreated(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectCreatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectCreated(projectId, projectName, ownerId),
            Times.Once);
    }
}

public class ProjectUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectUpdated_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var modifierId = 5L;
        var notification = new ProjectUpdatedEvent(projectId, "Updated Name", modifierId);

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectUpdated(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectUpdatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectUpdated(projectId, modifierId),
            Times.Once);
    }
}

public class ProjectArchivedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectArchived_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var modifierId = 5L;
        var notification = new ProjectArchivedEvent(projectId, "Test Project", modifierId);

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectArchived(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectArchivedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectArchived(projectId, modifierId),
            Times.Once);
    }
}

public class ProjectMemberAddedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectMemberAdded_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var projectName = "Test Project";
        var newMemberId = 10L;
        var inviterUserId = 5L;
        var notification = new ProjectMemberAddedEvent(projectId, projectName, newMemberId, ProjectRole.Contributor, inviterUserId);

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectMemberAdded(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectMemberAddedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectMemberAdded(projectId, newMemberId, projectName, inviterUserId),
            Times.Once);
    }
}

public class ProjectMemberRemovedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectMemberRemoved_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var removedUserId = 10L;
        var removingUserId = 5L;
        var notification = new ProjectMemberRemovedEvent(projectId, removedUserId, ProjectRole.Contributor, removingUserId);

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectMemberRemoved(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectMemberRemovedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectMemberRemoved(projectId, removedUserId, removingUserId),
            Times.Once);
    }
}

public class ProjectMemberUpdatedEventHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallNotifyProjectMemberUpdated_WithCorrectParameters()
    {
        // Arrange
        var projectId = 1L;
        var updatedMemberId = 10L;
        var inviterUserId = 5L;
        var notification = new ProjectMemberUpdatedEvent(projectId, "Test Project", updatedMemberId, ProjectRole.Contributor, ProjectRole.Admin, inviterUserId);

        var notificationServiceMock = new Mock<IProjectNotificationService>();
        notificationServiceMock
            .Setup(s => s.NotifyProjectMemberUpdated(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .Returns(Task.CompletedTask);

        var handler = new ProjectMemberUpdatedEventHandler(notificationServiceMock.Object);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyProjectMemberUpdated(projectId, updatedMemberId, inviterUserId),
            Times.Once);
    }
}
