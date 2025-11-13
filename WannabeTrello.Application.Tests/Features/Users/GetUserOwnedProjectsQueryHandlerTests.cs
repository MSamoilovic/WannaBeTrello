using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetUserOwnedProjects;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetUserOwnedProjectsQueryHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetUserOwnedProjectsQueryHandler _handler;

    public GetUserOwnedProjectsQueryHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetUserOwnedProjectsQueryHandler(_userServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndOwnsProjects_ShouldReturnOwnedProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var ownedProjects = new List<Project>
        {
            Project.Create("My Project 1", "Description 1", targetUserId),
            Project.Create("My Project 2", "Description 2", targetUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownedProjects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.OwnedProjects);
        Assert.Equal(2, response.OwnedProjects.Count);
        Assert.Equal("My Project 1", response.OwnedProjects[0].Name);
        Assert.Equal("My Project 2", response.OwnedProjects[1].Name);
        Assert.All(response.OwnedProjects, p => Assert.Equal(targetUserId, p.OwnerId));

        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserOwnsNoProjects_ShouldReturnEmptyList()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var ownedProjects = new List<Project>();

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownedProjects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.OwnedProjects);
        Assert.Empty(response.OwnedProjects);

        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestingOtherUserOwnedProjects_ShouldReturnTheirOwnedProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long otherUserId = 456L;

        var query = new GetUserOwnedProjectsQuery(otherUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var ownedProjects = new List<Project>
        {
            Project.Create("Other User's Project", "Description", otherUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownedProjects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.OwnedProjects);
        Assert.Equal("Other User's Project", response.OwnedProjects[0].Name);
        Assert.Equal(otherUserId, response.OwnedProjects[0].OwnerId);

        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(otherUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenUserOwnsSingleProject_ShouldReturnSingleProject()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var ownedProjects = new List<Project>
        {
            Project.Create("Single Project", "Only project owned", targetUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownedProjects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.OwnedProjects);
        Assert.Equal("Single Project", response.OwnedProjects[0].Name);
        Assert.Equal(targetUserId, response.OwnedProjects[0].OwnerId);

        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserOwnsMultipleProjects_ShouldReturnAllOwnedProjects()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserOwnedProjectsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var ownedProjects = new List<Project>
        {
            Project.Create("Project Alpha", "First project", targetUserId),
            Project.Create("Project Beta", "Second project", targetUserId),
            Project.Create("Project Gamma", "Third project", targetUserId),
            Project.Create("Project Delta", "Fourth project", targetUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownedProjects);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(4, response.OwnedProjects.Count);
        Assert.All(response.OwnedProjects, p => Assert.Equal(targetUserId, p.OwnerId));

        _userServiceMock.Verify(s => s.GetUserOwnedProjectsAsync(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

