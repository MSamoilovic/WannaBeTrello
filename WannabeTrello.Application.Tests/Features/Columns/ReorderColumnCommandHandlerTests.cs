using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Columns.ReorderColumn;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Columns;

public class ReorderColumnCommandHandlerTests
{
     [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCallServiceAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var boardId = 1L;
        var columnOrders = new Dictionary<long, int> { { 10L, 1 }, { 11L, 2 } };
        var command = new ReorderColumnCommand { BoardId = boardId, ColumnOrders = columnOrders };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.ReorderColumnsAsync(boardId, columnOrders, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new ReorderColumnCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(boardId, response.Result.Value);
        Assert.Equal("Columns reordered successfully", response.Result.Message);

        boardServiceMock.Verify(s => s.ReorderColumnsAsync(boardId, columnOrders, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ReorderColumnCommand();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new ReorderColumnCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ReorderColumnCommand();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new ReorderColumnCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var boardId = 999L;
        var columnOrders = new Dictionary<long, int>();
        var command = new ReorderColumnCommand { BoardId = boardId, ColumnOrders = columnOrders };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.ReorderColumnsAsync(boardId, columnOrders, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Domain.Entities.Board), boardId));

        var handler = new ReorderColumnCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}