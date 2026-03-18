using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Activities.GetActivityByProject;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Tests.Features.Activities;

public class GetActivityByProjectQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndActivitiesExist_ShouldReturnActivities()
    {
        // Arrange
        var userId = 123L;
        var projectId = 789L;
        var query = new GetActivityByProjectQuery(projectId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activity1 = new Activity(
            ActivityType.ProjectCreated,
            "Project was created",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "ProjectName", "Test Project" } }
        );

        var activity2 = new Activity(
            ActivityType.ProjectMemberAdded,
            "Member was added to project",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "MemberName", "John Doe" } }
        );

        var activities = new List<Activity> { activity1, activity2 };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                factory());

        var handler = new GetActivityByProjectQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(ActivityType.ProjectCreated.ToString(), response[0].Type);
        Assert.Equal("Project was created", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal(ActivityType.ProjectMemberAdded.ToString(), response[1].Type);
        Assert.Equal("Member was added to project", response[1].Description);
        
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForProjectAsync(projectId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndNoActivitiesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 123L;
        var projectId = 789L;
        var query = new GetActivityByProjectQuery(projectId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Activity>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                factory());

        var handler = new GetActivityByProjectQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForProjectAsync(projectId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var projectId = 789L;
        var query = new GetActivityByProjectQuery(projectId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetActivityByProjectQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForProjectAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var projectId = 789L;
        var query = new GetActivityByProjectQuery(projectId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetActivityByProjectQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForProjectAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActivitiesHaveOldAndNewValues_ShouldMapCorrectly()
    {
        // Arrange
        var userId = 123L;
        var projectId = 789L;
        var query = new GetActivityByProjectQuery(projectId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var oldValue = new Dictionary<string, object?> { { "Name", "Old Project Name" } };
        var newValue = new Dictionary<string, object?> { { "Name", "New Project Name" } };

        var activity = new Activity(
            ActivityType.ProjectUpdated,
            "Project was updated",
            userId,
            oldValue,
            newValue
        );

        var activities = new List<Activity> { activity };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                factory());

        var handler = new GetActivityByProjectQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ActivityType.ProjectUpdated.ToString(), response[0].Type);
        Assert.Equal("Project was updated", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal("Old Project Name", response[0].OldValue["Name"]);
        Assert.Equal("New Project Name", response[0].NewValue["Name"]);
        Assert.NotNull(response[0].Timestamp);
    }
}

