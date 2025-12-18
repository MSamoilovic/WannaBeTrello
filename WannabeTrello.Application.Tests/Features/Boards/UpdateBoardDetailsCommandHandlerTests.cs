using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.UpdateBoard;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;

public class UpdateBoardDetailsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldUpdateBoardAndReturnResponse()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        const long projectId = 789L;
        var command = new UpdateBoardCommand { Id = boardId, Name = "Updated Name", Description = "Updated Desc" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        var updatedBoardFromService = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        ApplicationTestUtils.SetPrivatePropertyValue(updatedBoardFromService, nameof(Board.Id), boardId);
        ApplicationTestUtils.SetPrivatePropertyValue(updatedBoardFromService, nameof(Board.ProjectId), projectId);
        
        boardServiceMock
            .Setup(s => s.UpdateBoardAsync(command.Id, command.Name, command.Description, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBoardFromService);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(boardId, response.Id);

        boardServiceMock.Verify(s => s.UpdateBoardAsync(command.Id, command.Name, command.Description, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(s => s.RemoveAsync(CacheKeys.Board(boardId), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(s => s.RemoveAsync(CacheKeys.ProjectBoards(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateBoardCommand { Id = 1L };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated.", exception.Message);
        boardServiceMock.Verify(s => s.UpdateBoardAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateBoardCommand { Id = 1L };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardServiceMock = new Mock<IBoardService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated.", exception.Message);
        cacheServiceMock.Verify(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        const long userId = 123L;
        const long nonExistentBoardId = 999L;
        var command = new UpdateBoardCommand { Id = nonExistentBoardId, Name = "Test" };
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.UpdateBoardAsync(nonExistentBoardId, command.Name, command.Description, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Board), nonExistentBoardId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal($"Entity \'Board\' ({nonExistentBoardId}) was not found.", exception.Message);
        cacheServiceMock.Verify(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}