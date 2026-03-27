using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Events.Comment;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Events.Comment;

public class UserMentionedInCommentEventHandlerTests
{
    private static User CreateUser(long id, string username, bool isActive = true)
    {
        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, "Id", id);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "UserName", username);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "IsActive", isActive);
        return user;
    }

    [Fact]
    public async Task Handle_WithValidMentions_ShouldNotifyMentionedUsers()
    {
        // Arrange
        var taskId = 1L;
        var mentionedByUserId = 10L;
        var mentionedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "alice", "bob" };
        var notification = new UserMentionedInCommentEvent(taskId, mentionedByUserId, mentionedUsernames);

        var alice = CreateUser(20L, "alice");
        var bob = CreateUser(30L, "bob");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.SearchUsers())
            .Returns(new TestAsyncEnumerable<User>([alice, bob]));

        var notificationServiceMock = new Mock<IUserNotificationService>();
        var logger = NullLogger<UserMentionedInCommentEventHandler>.Instance;
        var handler = new UserMentionedInCommentEventHandler(
            userRepositoryMock.Object, notificationServiceMock.Object, logger);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(20L, taskId, mentionedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(30L, taskId, mentionedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldNotNotifyAuthorEvenIfMentioned()
    {
        // Arrange — autor je mencionovao samog sebe
        var taskId = 1L;
        var authorUserId = 10L;
        var mentionedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "selfmention" };
        var notification = new UserMentionedInCommentEvent(taskId, authorUserId, mentionedUsernames);

        var author = CreateUser(authorUserId, "selfmention");

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.SearchUsers())
            .Returns(new TestAsyncEnumerable<User>([author]));

        var notificationServiceMock = new Mock<IUserNotificationService>();
        var logger = NullLogger<UserMentionedInCommentEventHandler>.Instance;
        var handler = new UserMentionedInCommentEventHandler(
            userRepositoryMock.Object, notificationServiceMock.Object, logger);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert — nijedna notifikacija ne bi trebala biti poslana
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoUsersMatchMentionedUsernames_ShouldNotCallNotificationService()
    {
        // Arrange
        var mentionedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nonexistent" };
        var notification = new UserMentionedInCommentEvent(1L, 10L, mentionedUsernames);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.SearchUsers())
            .Returns(new TestAsyncEnumerable<User>([]));

        var notificationServiceMock = new Mock<IUserNotificationService>();
        var logger = NullLogger<UserMentionedInCommentEventHandler>.Instance;
        var handler = new UserMentionedInCommentEventHandler(
            userRepositoryMock.Object, notificationServiceMock.Object, logger);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldNotNotifyInactiveUser()
    {
        // Arrange
        var taskId = 1L;
        var mentionedByUserId = 10L;
        var mentionedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "inactive" };
        var notification = new UserMentionedInCommentEvent(taskId, mentionedByUserId, mentionedUsernames);

        var inactiveUser = CreateUser(20L, "inactive", isActive: false);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.SearchUsers())
            .Returns(new TestAsyncEnumerable<User>([inactiveUser]));

        var notificationServiceMock = new Mock<IUserNotificationService>();
        var logger = NullLogger<UserMentionedInCommentEventHandler>.Instance;
        var handler = new UserMentionedInCommentEventHandler(
            userRepositoryMock.Object, notificationServiceMock.Object, logger);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMixedActiveAndInactiveUsers_ShouldNotifyOnlyActiveUsers()
    {
        // Arrange
        var taskId = 1L;
        var mentionedByUserId = 10L;
        var mentionedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "active", "inactive" };
        var notification = new UserMentionedInCommentEvent(taskId, mentionedByUserId, mentionedUsernames);

        var activeUser = CreateUser(20L, "active", isActive: true);
        var inactiveUser = CreateUser(30L, "inactive", isActive: false);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.SearchUsers())
            .Returns(new TestAsyncEnumerable<User>([activeUser, inactiveUser]));

        var notificationServiceMock = new Mock<IUserNotificationService>();
        var logger = NullLogger<UserMentionedInCommentEventHandler>.Instance;
        var handler = new UserMentionedInCommentEventHandler(
            userRepositoryMock.Object, notificationServiceMock.Object, logger);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert — samo aktivni korisnik dobija notifikaciju
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(20L, taskId, mentionedByUserId, It.IsAny<CancellationToken>()),
            Times.Once);
        notificationServiceMock.Verify(
            s => s.NotifyUserMentioned(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
