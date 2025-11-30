using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Activities.GetActivityByTask;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Tests.Features.Activities;

public class GetActivityByTaskQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndActivitiesExist_ShouldReturnActivities()
    {
        // Arrange
        var userId = 123L;
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activity1 = new Activity(
            ActivityType.TaskCreated,
            "Task was created",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "TaskName", "Test Task" } }
        );

        var activity2 = new Activity(
            ActivityType.TaskUpdated,
            "Task was updated",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "TaskName", "Updated Task" } }
        );

        var activity3 = new Activity(
            ActivityType.CommentAdded,
            "Comment was added to task",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "CommentId", 1 } }
        );

        var activities = new List<Activity> { activity1, activity2, activity3 };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
        Assert.Equal(ActivityType.TaskCreated.ToString(), response[0].Type);
        Assert.Equal("Task was created", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal(ActivityType.TaskUpdated.ToString(), response[1].Type);
        Assert.Equal("Task was updated", response[1].Description);
        Assert.Equal(ActivityType.CommentAdded.ToString(), response[2].Type);
        Assert.Equal("Comment was added to task", response[2].Description);
        
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndNoActivitiesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 123L;
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Activity>());

        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForTaskAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForTaskAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActivitiesHaveOldAndNewValues_ShouldMapCorrectly()
    {
        // Arrange
        var userId = 123L;
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var oldValue = new Dictionary<string, object?> { { "Status", "InProgress" }, { "Priority", "Low" } };
        var newValue = new Dictionary<string, object?> { { "Status", "Completed" }, { "Priority", "High" } };

        var activity = new Activity(
            ActivityType.TaskUpdated,
            "Task status and priority were updated",
            userId,
            oldValue,
            newValue
        );

        var activities = new List<Activity> { activity };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ActivityType.TaskUpdated.ToString(), response[0].Type);
        Assert.Equal("Task status and priority were updated", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal("InProgress", response[0].OldValue["Status"]);
        Assert.Equal("Low", response[0].OldValue["Priority"]);
        Assert.Equal("Completed", response[0].NewValue["Status"]);
        Assert.Equal("High", response[0].NewValue["Priority"]);
        Assert.NotNull(response[0].Timestamp);
    }

    [Fact]
    public async Task Handle_WhenTaskHasMultipleActivityTypes_ShouldReturnAllActivities()
    {
        // Arrange
        var userId = 123L;
        var taskId = 101L;
        var query = new GetActivityByTaskQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activities = new List<Activity>
        {
            new Activity(ActivityType.TaskCreated, "Task created", userId),
            new Activity(ActivityType.TaskAssigned, "Task assigned", userId),
            new Activity(ActivityType.TaskMoved, "Task moved", userId),
            new Activity(ActivityType.TaskCompleted, "Task completed", userId)
        };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForTaskAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var handler = new GetActivityByTaskQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(4, response.Count);
        Assert.Equal(ActivityType.TaskCreated.ToString(), response[0].Type);
        Assert.Equal(ActivityType.TaskAssigned.ToString(), response[1].Type);
        Assert.Equal(ActivityType.TaskMoved.ToString(), response[2].Type);
        Assert.Equal(ActivityType.TaskCompleted.ToString(), response[3].Type);
    }
}

