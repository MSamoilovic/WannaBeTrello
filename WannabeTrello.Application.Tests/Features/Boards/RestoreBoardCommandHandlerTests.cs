using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.RestoreBoard;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;

public class RestoreBoardCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldRestoreBoardAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var projectId = 789L;
        var command = new RestoreBoardCommand(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var board = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.Id), boardId);
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.ProjectId), projectId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.GetBoardWithDetailsAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);
        boardServiceMock
            .Setup(s => s.RestoreBoardAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new RestoreBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(boardId, response.Result.Value);
        Assert.Equal($"Board {boardId} is now restored.", response.Result.Message);

        boardServiceMock.Verify(s => s.RestoreBoardAsync(boardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new RestoreBoardCommand(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new RestoreBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        boardServiceMock.Verify(s => s.RestoreBoardAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new RestoreBoardCommand(1L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null); // UserId je null

        var boardServiceMock = new Mock<IBoardService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new RestoreBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
        
        cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentBoardId = 999L;
        var command = new RestoreBoardCommand(nonExistentBoardId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.GetBoardWithDetailsAsync(nonExistentBoardId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Domain.Entities.Board), nonExistentBoardId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new RestoreBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
        
        cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenBoardIsRestored_ShouldInvalidateCorrectCacheKeys()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var projectId = 789L;
        var command = new RestoreBoardCommand(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var board = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.Id), boardId);
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.ProjectId), projectId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.GetBoardWithDetailsAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);
        boardServiceMock
            .Setup(s => s.RestoreBoardAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new RestoreBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        cacheServiceMock.Verify(c => c.RemoveAsync($"board:{boardId}", It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync($"project:{projectId}:boards", It.IsAny<CancellationToken>()), Times.Once);
    }
}