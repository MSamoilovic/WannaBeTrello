using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;

public class GetColumnsByBoardIdQueryTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndBoardExists_ShouldReturnColumns()
    {
        // Arrange
        var userId = 123L;
        var boardId = 1L;
        var query = new GetColumnsByBoardIdQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        var columnsFromService = new List<Column>
        {
            ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>(),
            ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>()
        };
        
        foreach (var col in columnsFromService)
        {
            ApplicationTestUtils.SetPrivatePropertyValue(col, "_tasks", new List<BoardTask>());
        }
        
        boardServiceMock
            .Setup(s => s.GetColumnsByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(columnsFromService);

        var cachingServiceMock = new Mock<ICacheService>();
        cachingServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Column>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Column>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetColumnsByBoardIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object, cachingServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        boardServiceMock.Verify(s => s.GetColumnsByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetColumnsByBoardIdQuery(1L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var cachingServiceMock = new Mock<ICacheService>();

        var handler = new GetColumnsByBoardIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object, cachingServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var query = new GetColumnsByBoardIdQuery(1L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null); // UserId je null

        var boardServiceMock = new Mock<IBoardService>();
        var cachingServiceMock = new Mock<ICacheService>();

        var handler = new GetColumnsByBoardIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object, cachingServiceMock.Object);

        // Act & Assert
        // Proveravamo InvalidOperationException jer će '!.Value' na nullable tipu baciti ovu grešku.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentBoardId = 999L;
        var query = new GetColumnsByBoardIdQuery(nonExistentBoardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.GetColumnsByBoardIdAsync(nonExistentBoardId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Board), nonExistentBoardId));

        var cachingServiceMock = new Mock<ICacheService>();
        cachingServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Column>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Column>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetColumnsByBoardIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object, cachingServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
}