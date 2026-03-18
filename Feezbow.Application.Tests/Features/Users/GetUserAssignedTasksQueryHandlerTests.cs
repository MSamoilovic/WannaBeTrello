using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetUserAssignedTasks;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetUserAssignedTasksQueryHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetUserAssignedTasksQueryHandler _handler;

    public GetUserAssignedTasksQueryHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetUserAssignedTasksQueryHandler(_userServiceMock.Object, _currentUserServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndHasTasks_ShouldReturnUserTasks()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var tasks = new List<BoardTask>
        {
            BoardTask.Create("Task 1", "Description 1", TaskPriority.High, DateTime.UtcNow.AddDays(7), 1, 1L, currentUserId, currentUserId),
            BoardTask.Create("Task 2", "Description 2", TaskPriority.Medium, DateTime.UtcNow.AddDays(14), 2, 2L, currentUserId, currentUserId)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Tasks);
        Assert.Equal(2, response.Tasks.Count);
        Assert.Equal("Task 1", response.Tasks[0].Title);
        Assert.Equal("Task 2", response.Tasks[1].Title);
        Assert.Equal(TaskPriority.High, response.Tasks[0].Priority);
        Assert.Equal(TaskPriority.Medium, response.Tasks[1].Priority);

        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.Is<string>(k => k == CacheKeys.UserTasks(targetUserId)),
            It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoTasks_ShouldReturnEmptyList()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var tasks = new List<BoardTask>();

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Tasks);
        Assert.Empty(response.Tasks);

        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestingOtherUserTasks_ShouldReturnTheirTasks()
    {
        // Arrange
        const long currentUserId = 123L;
        const long otherUserId = 456L;

        var query = new GetUserAssignedTasksQuery(otherUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var tasks = new List<BoardTask>
        {
            BoardTask.Create("Other User Task", "Description", TaskPriority.Low, null, 1, 1L, otherUserId, currentUserId)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        _userServiceMock
            .Setup(s => s.GetUserAssignedTasksAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Tasks);
        Assert.Equal("Other User Task", response.Tasks[0].Title);

        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(otherUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenTasksHaveMixedPriorities_ShouldReturnAllTasks()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var tasks = new List<BoardTask>
        {
            BoardTask.Create("High Priority Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(1), 1, 1L, currentUserId, currentUserId),
            BoardTask.Create("Medium Priority Task", "Description", TaskPriority.Medium, DateTime.UtcNow.AddDays(5), 2, 1L, currentUserId, currentUserId),
            BoardTask.Create("Low Priority Task", "Description", TaskPriority.Low, null, 3, 2L, currentUserId, currentUserId)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Tasks.Count);
        Assert.Contains(response.Tasks, t => t.Priority == TaskPriority.High);
        Assert.Contains(response.Tasks, t => t.Priority == TaskPriority.Medium);
        Assert.Contains(response.Tasks, t => t.Priority == TaskPriority.Low);

        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTasksHaveDueDates_ShouldIncludeDueDateInformation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserAssignedTasksQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var futureDate = DateTime.UtcNow.AddDays(10);
        var tasks = new List<BoardTask>
        {
            BoardTask.Create("Task with Due Date", "Description", TaskPriority.High, futureDate, 1, 1L, currentUserId, currentUserId),
            BoardTask.Create("Task without Due Date", "Description", TaskPriority.Medium, null, 2, 1L, currentUserId, currentUserId)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Tasks.Count);
        Assert.NotNull(response.Tasks[0].DueDate);
        Assert.Null(response.Tasks[1].DueDate);

        _userServiceMock.Verify(s => s.GetUserAssignedTasksAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

