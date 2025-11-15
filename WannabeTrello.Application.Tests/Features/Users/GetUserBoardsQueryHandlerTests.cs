using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetUserBoards;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetUserBoardsQueryHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetUserBoardsQueryHandler _handler;

    public GetUserBoardsQueryHandlerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetUserBoardsQueryHandler(_userServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndHasBoards_ShouldReturnUserBoards()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boards = new List<Board>
        {
            Board.Create("Board 1", "Description 1", 1L, currentUserId),
            Board.Create("Board 2", "Description 2", 2L, currentUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Boards);
        Assert.Equal(2, response.Boards.Count);
        Assert.Equal("Board 1", response.Boards[0].Name);
        Assert.Equal("Board 2", response.Boards[1].Name);

        _userServiceMock.Verify(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoBoards_ShouldReturnEmptyList()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boards = new List<Board>();

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Boards);
        Assert.Empty(response.Boards);

        _userServiceMock.Verify(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserBoardMemberships(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long targetUserId = 123L;
        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(query, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        _userServiceMock.Verify(s => s.GetUserBoardMemberships(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestingOtherUserBoards_ShouldReturnTheirBoards()
    {
        // Arrange
        const long currentUserId = 123L;
        const long otherUserId = 456L;

        var query = new GetUserBoardsQuery(otherUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boards = new List<Board>
        {
            Board.Create("Other User Board", "Description", 1L, otherUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Boards);
        Assert.Equal("Other User Board", response.Boards[0].Name);

        _userServiceMock.Verify(s => s.GetUserBoardMemberships(otherUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token));
    }

    [Fact]
    public async Task Handle_WhenUserHasMixedBoardRoles_ShouldReturnAllBoards()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boards = new List<Board>
        {
            Board.Create("Admin Board", "Description 1", 1L, targetUserId),
            Board.Create("Member Board", "Description 2", 2L, 999L),
            Board.Create("Viewer Board", "Description 3", 3L, 888L)
        };

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Boards.Count);

        _userServiceMock.Verify(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBoardsHaveProjects_ShouldIncludeProjectInformation()
    {
        // Arrange
        const long currentUserId = 123L;
        const long targetUserId = 123L;

        var query = new GetUserBoardsQuery(targetUserId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boards = new List<Board>
        {
            Board.Create("Board with Project", "Description", 1L, currentUserId)
        };

        _userServiceMock
            .Setup(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boards);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Boards);
        Assert.Equal(1L, response.Boards[0].ProjectId);

        _userServiceMock.Verify(s => s.GetUserBoardMemberships(targetUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

