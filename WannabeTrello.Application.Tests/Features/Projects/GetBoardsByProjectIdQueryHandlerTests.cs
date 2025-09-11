﻿using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.GetBoardsByProjectId;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class GetBoardsByProjectIdQueryHandlerTests
{
    private readonly Mock<IBoardService> _boardServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetBoardsByProjectIdQueryHandler _handler;

    public GetBoardsByProjectIdQueryHandlerTests()
    {
        _boardServiceMock = new Mock<IBoardService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetBoardsByProjectIdQueryHandler(
            _boardServiceMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_AuthenticatedUserAndBoardsExist_ReturnsListOfBoardDtos()
    {
        // ARRANGE
        const int projectId = 1;
        const int userId = 123;

        var board1 = Board.Create("Board A", "Desc A", projectId, userId);
        var board2 = Board.Create("Board B", "Desc B", projectId, userId);

        var mockBoards = new List<Board>
        {
            board1,
            board2
        };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _boardServiceMock.Setup(x => x.GetBoardByProjectIdAsync(projectId, userId, CancellationToken.None))
            .ReturnsAsync(mockBoards);

        var query = new GetBoardsByProjectIdQuery(projectId);

        var result = await _handler.Handle(query, CancellationToken.None);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            board =>
            {
                Assert.Equal(board1.Id, board.BoardId);
                Assert.Equal(board1.Name, board.BoardName);
            },
            // Proveri drugi board
            board =>
            {
                Assert.Equal(board2.Id, board.BoardId);
                Assert.Equal(board2.Name, board.BoardName);
            }
        );

        _boardServiceMock.Verify(x => x.GetBoardByProjectIdAsync(projectId, userId, CancellationToken.None), Times.Once);
    }


    [Fact]
    public async Task Handle_AuthenticatedUserAndNoBoardsExist_ReturnsEmptyList()
    {
        // ARRANGE
        var projectId = 1;
        var userId = 123;

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _boardServiceMock.Setup(x => x.GetBoardByProjectIdAsync(projectId, userId, CancellationToken.None))
            .ReturnsAsync(new List<Board>());

        var query = new GetBoardsByProjectIdQuery(projectId);

        // ACT
        var result = await _handler.Handle(query, CancellationToken.None);

        // ASSERT
        Assert.NotNull(result);
        Assert.Empty(result);

        _boardServiceMock.Verify(x => x.GetBoardByProjectIdAsync(projectId, userId, CancellationToken.None), Times.Once);
    }


    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        var projectId = 1;
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var query = new GetBoardsByProjectIdQuery(projectId);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(query, CancellationToken.None));

        _boardServiceMock.Verify(x => x.GetBoardByProjectIdAsync(It.IsAny<long>(), It.IsAny<long>(), CancellationToken.None),
            Times.Never);
    }
}