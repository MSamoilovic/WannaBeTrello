using System.Reflection;
using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.UpdateBoard;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;

public class UpdateBoardDetailsCommandHandlerTests
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
    public async Task Handle_WhenUserIsAuthenticated_ShouldUpdateBoardAndReturnResponse()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        var command = new UpdateBoardCommand { Id = boardId, Name = "Updated Name", Description = "Updated Desc" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        var updatedBoardFromService = CreateInstanceWithoutConstructor<Board>();
        SetPrivatePropertyValue(updatedBoardFromService, nameof(Board.Id), boardId);
        
        boardServiceMock
            .Setup(s => s.UpdateBoardDetailsAsync(command.Id, command.Name, command.Description, userId))
            .ReturnsAsync(updatedBoardFromService);

        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(boardId, response.Id);

        boardServiceMock.Verify(s => s.UpdateBoardDetailsAsync(command.Id, command.Name, command.Description, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateBoardCommand { Id = 1L };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated.", exception.Message);
        boardServiceMock.Verify(s => s.UpdateBoardDetailsAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
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
        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal("User is not authenticated.", exception.Message);
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
            .Setup(s => s.UpdateBoardDetailsAsync(nonExistentBoardId, command.Name, command.Description, userId))
            .ThrowsAsync(new NotFoundException(nameof(Board), nonExistentBoardId));

        var handler = new UpdateBoardCommandHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
            
        Assert.Equal($"Entitet \"Board\" ({nonExistentBoardId}) nije pronađen.", exception.Message);
    }
}