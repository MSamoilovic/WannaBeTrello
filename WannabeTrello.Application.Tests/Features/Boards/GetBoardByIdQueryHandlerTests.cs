using System.Reflection;
using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.GetBoardById;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Appilcation.Tests.Features.Boards;

public class GetBoardByIdQueryHandlerTests
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
    
    private static void SetPrivateFieldValue<T>(T obj, string fieldName, object value)
    {
        var field = typeof(T).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            throw new InvalidOperationException($"Private field '{fieldName}' not found on type '{typeof(T).Name}'.");
        }
        
        field.SetValue(obj, value);
    }
    
    private static T CreateInstanceWithoutConstructor<T>() where T : class
    {
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
    }
    
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndBoardExists_ShouldReturnBoardData()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 456L;
        var query = new GetBoardByIdQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        var boardFromService = CreateInstanceWithoutConstructor<Board>();
        SetPrivatePropertyValue(boardFromService, nameof(Board.Id), boardId);
        SetPrivatePropertyValue(boardFromService, nameof(Board.Name), "Test Board");
        
        SetPrivateFieldValue(boardFromService, "_columns", new List<Column>());

        boardServiceMock
            .Setup(s => s.GetBoardByIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardFromService);

        var handler = new GetBoardByIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(boardId, response.Id);
        Assert.Equal("Test Board", response.Name);

        boardServiceMock.Verify(s => s.GetBoardByIdAsync(boardId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowAccessDeniedException()
    {
        // Arrange
        const long boardId = 1L;
        var query = new GetBoardByIdQuery(boardId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new GetBoardByIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        boardServiceMock.Verify(s => s.GetBoardByIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowAccessDeniedException()
    {
        // Arrange
        const long boardId = 1L;
        var query = new GetBoardByIdQuery(boardId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new GetBoardByIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
    }
    
    [Fact]
    public async Task Handle_WhenBoardDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        const long userId = 123L;
        const long boardId = 999L;
        var query = new GetBoardByIdQuery(boardId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        boardServiceMock
            .Setup(s => s.GetBoardByIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Board), boardId)); 

        var handler = new GetBoardByIdQueryHandler(boardServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal($"Entitet \"Board\" ({boardId}) nije pronađen.", exception.Message);
    }
}