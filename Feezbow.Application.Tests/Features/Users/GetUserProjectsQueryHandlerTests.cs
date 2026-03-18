using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetUserProjects;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetUserProjectsQueryHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetUserProjectsQueryHandler _handler;

    public GetUserProjectsQueryHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetUserProjectsQueryHandler(_userServiceMock.Object, _currentUserServiceMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndHasProjects_ShouldReturnUserProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var projects = new List<Project>
        {
            Project.Create("Project 1", "Description 1", 123L),
            Project.Create("Project 2", "Description 2", 456L)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Project>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Projects);
        Assert.Equal(2, response.Projects.Count);
        Assert.Equal("Project 1", response.Projects[0].Name);
        Assert.Equal("Project 2", response.Projects[1].Name);

        _userServiceMock.Verify(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.Is<string>(k => k == CacheKeys.UserProjects(targetUserId)),
            It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoProjects_ShouldReturnEmptyList()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var projects = new List<Project>();

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Project>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Projects);
        Assert.Empty(response.Projects);

        _userServiceMock.Verify(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserProjectsAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserProjectsAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestingOtherUserProjects_ShouldReturnTheirProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long otherUserId = 456L;

        var query = new GetUserProjectsQuery(otherUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var projects = new List<Project>
        {
            Project.Create("Other User Project", "Description", otherUserId)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Project>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        _userServiceMock
            .Setup(s => s.GetUserProjectsAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Projects);
        Assert.Equal("Other User Project", response.Projects[0].Name);

        _userServiceMock.Verify(s => s.GetUserProjectsAsync(otherUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenUserHasMixedProjectRoles_ShouldReturnAllProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var projects = new List<Project>
        {
            Project.Create("Owner Project", "Description 1", targetUserId),
            Project.Create("Member Project", "Description 2", 999L),
            Project.Create("Admin Project", "Description 3", 888L)
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Project>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Project>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
        
        _userServiceMock
            .Setup(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Projects.Count);

        _userServiceMock.Verify(s => s.GetUserProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

