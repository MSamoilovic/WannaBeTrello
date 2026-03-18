using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Activities.GetActivityByBoard;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Tests.Features.Activities;

public class GetActivityByBoardQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndActivitiesExist_ShouldReturnActivities()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        var query = new GetActivityByBoardQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activity1 = new Activity(
            ActivityType.BoardCreated,
            "Board was created",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "BoardName", "Test Board" } }
        );

        var activity2 = new Activity(
            ActivityType.TaskCreated,
            "Task was created",
            userId,
            new Dictionary<string, object?>(),
            new Dictionary<string, object?> { { "TaskName", "Test Task" } }
        );

        var activities = new List<Activity> { activity1, activity2 };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForBoardAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                Task.FromResult(factory().GetAwaiter().GetResult()));

        var handler = new GetActivityByBoardQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(ActivityType.BoardCreated.ToString(), response[0].Type);
        Assert.Equal("Board was created", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal(ActivityType.TaskCreated.ToString(), response[1].Type);
        Assert.Equal("Task was created", response[1].Description);
        
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForBoardAsync(boardId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndNoActivitiesExist_ShouldReturnEmptyList()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        var query = new GetActivityByBoardQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForBoardAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Activity>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                Task.FromResult(factory().GetAwaiter().GetResult()));

        var handler = new GetActivityByBoardQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForBoardAsync(boardId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var boardId = 456L;
        var query = new GetActivityByBoardQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetActivityByBoardQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForBoardAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var boardId = 456L;
        var query = new GetActivityByBoardQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var activityLogServiceMock = new Mock<IActivityLogService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetActivityByBoardQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        activityLogServiceMock.Verify(
            s => s.GetActivitiesForBoardAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActivitiesHaveOldAndNewValues_ShouldMapCorrectly()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        var query = new GetActivityByBoardQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var oldValue = new Dictionary<string, object?> { { "Name", "Old Board Name" } };
        var newValue = new Dictionary<string, object?> { { "Name", "New Board Name" } };

        var activity = new Activity(
            ActivityType.BoardUpdated,
            "Board was updated",
            userId,
            oldValue,
            newValue
        );

        var activities = new List<Activity> { activity };

        var activityLogServiceMock = new Mock<IActivityLogService>();
        activityLogServiceMock
            .Setup(s => s.GetActivitiesForBoardAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IEnumerable<Activity>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns((string key, Func<Task<IEnumerable<Activity>>> factory, TimeSpan? expiration, CancellationToken ct) => 
                Task.FromResult(factory().GetAwaiter().GetResult()));

        var handler = new GetActivityByBoardQueryHandler(activityLogServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ActivityType.BoardUpdated.ToString(), response[0].Type);
        Assert.Equal("Board was updated", response[0].Description);
        Assert.Equal(userId, response[0].UserId);
        Assert.Equal("Old Board Name", response[0].OldValue["Name"]);
        Assert.Equal("New Board Name", response[0].NewValue["Name"]);
    }
}

