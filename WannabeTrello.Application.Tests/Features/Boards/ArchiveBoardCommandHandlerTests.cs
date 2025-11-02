using System.Reflection;
using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.ArchiveBoard;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;

public class ArchiveBoardCommandHandlerTests
{
    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        var currentType = typeof(T).BaseType;
        while (currentType != null && property == null)
        {
            property = currentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            currentType = currentType.BaseType;
        }
        property?.SetValue(obj, value, null);
    }
    
    // Pomoćna metoda za kreiranje instance bez pozivanja konstruktora
    private static T CreateInstanceWithoutConstructor<T>() where T : class
    {
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldArchiveBoardAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var command = new ArchiveBoardCommand(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.ArchiveBoardAsync(boardId, userId, CancellationToken.None))
            .ReturnsAsync(boardId);

        var handler = new ArchiveBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(boardId, response.Result.Value);
        Assert.Equal($"Board {boardId} is now archived.", response.Result.Message);

        boardServiceMock.Verify(s => s.ArchiveBoardAsync(boardId, userId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ArchiveBoardCommand(1L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new ArchiveBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        boardServiceMock.Verify(s => s.ArchiveBoardAsync(It.IsAny<long>(), It.IsAny<long>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ArchiveBoardCommand(1L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new ArchiveBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
    }
    
    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentBoardId = 999L;
        var command = new ArchiveBoardCommand(nonExistentBoardId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.ArchiveBoardAsync(nonExistentBoardId, userId, CancellationToken.None))
            .ThrowsAsync(new NotFoundException(nameof(Board), nonExistentBoardId));

        var handler = new ArchiveBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal($"Entity \'Board\' ({nonExistentBoardId}) was not found.", exception.Message);
    }
}