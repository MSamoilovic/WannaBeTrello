using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Activities.GetActivityByUser;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Tests.Features.Activities;

public class GetActivityByUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndActivitiesExist_ShouldReturnActivities()
    {
        // Arrange
        var currentUserId = 123L;
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var activity1 = new Activity(
            ActivityType.ProjectCreated,
            "Project was created",
            targetUserId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "ProjectName", "Test Project" } }
        );

        var activity2 = new Activity(
            ActivityType.BoardCreated,
            "Board was created",
            targetUserId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "BoardName", "Test Board" } }
        );

        var activity3 = new Activity(
            ActivityType.TaskCreated,
            "Task was created",
            targetUserId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "TaskName", "Test Task" } }
        );

        var activities = new List<Activity> { activity1, activity2, activity3 };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
        Assert.Equal(ActivityType.ProjectCreated.ToString(), response[0].Type);
        Assert.Equal("Project was created", response[0].Description);
        Assert.Equal(targetUserId, response[0].UserId);
        Assert.Equal(ActivityType.BoardCreated.ToString(), response[1].Type);
        Assert.Equal("Board was created", response[1].Description);
        Assert.Equal(ActivityType.TaskCreated.ToString(), response[2].Type);
        Assert.Equal("Task was created", response[2].Description);
        
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndNoActivitiesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var currentUserId = 123L;
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Activity>());

        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForUserAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForUserAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActivitiesHaveOldAndNewValues_ShouldMapCorrectly()
    {
        // Arrange
        var currentUserId = 123L;
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var oldValue = new Dictionary<string, object?> { { "Email", "old@example.com" } };
        var newValue = new Dictionary<string, object?> { { "Email", "new@example.com" } };

        var activity = new Activity(
            ActivityType.UserProfileUpdated,
            "User profile was updated",
            targetUserId,
            oldValue,
            newValue
        );

        var activities = new List<Activity> { activity };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ActivityType.UserProfileUpdated.ToString(), response[0].Type);
        Assert.Equal("User profile was updated", response[0].Description);
        Assert.Equal(targetUserId, response[0].UserId);
        Assert.Equal("old@example.com", response[0].OldValue["Email"]);
        Assert.Equal("new@example.com", response[0].NewValue["Email"]);
        Assert.NotNull(response[0].Timestamp);
    }

    [Fact]
    public async Task Handle_WhenUserHasMultipleActivityTypes_ShouldReturnAllActivities()
    {
        // Arrange
        var currentUserId = 123L;
        var targetUserId = 456L;
        var query = new GetActivityByUserQuery(targetUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var activities = new List<Activity>
        {
            new Activity(ActivityType.ProjectCreated, "Project created", targetUserId),
            new Activity(ActivityType.BoardCreated, "Board created", targetUserId),
            new Activity(ActivityType.TaskCreated, "Task created", targetUserId),
            new Activity(ActivityType.CommentAdded, "Comment added", targetUserId),
            new Activity(ActivityType.UserProfileUpdated, "Profile updated", targetUserId)
        };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForUserAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByUserQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(5, response.Count);
        Assert.Equal(ActivityType.ProjectCreated.ToString(), response[0].Type);
        Assert.Equal(ActivityType.BoardCreated.ToString(), response[1].Type);
        Assert.Equal(ActivityType.TaskCreated.ToString(), response[2].Type);
        Assert.Equal(ActivityType.CommentAdded.ToString(), response[3].Type);
        Assert.Equal(ActivityType.UserProfileUpdated.ToString(), response[4].Type);
        
        // All activities should belong to the target user
        Assert.All(response, r => Assert.Equal(targetUserId, r.UserId));
    }
}

