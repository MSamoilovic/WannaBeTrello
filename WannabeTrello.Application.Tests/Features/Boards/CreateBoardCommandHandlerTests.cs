using System.Reflection;
using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.CreateBoard;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Boards;


public class CreateBoardCommandHandlerTests
{
   private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (property == null || !property.CanWrite)
        {
            var currentType = typeof(T).BaseType;
            while(currentType != null && property == null)
            {
                property = currentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                currentType = currentType.BaseType;
            }
        }
        
        property?.SetValue(obj, value, null);
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCreateBoardAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var newBoardId = 456L;
        var command = new CreateBoardCommand { ProjectId = 1, Name = "Test Board", Description = "Test Desc" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var boardServiceMock = new Mock<IBoardService>();
        var createdBoard = (Board)FormatterServices.GetUninitializedObject(typeof(Board));
        SetPrivatePropertyValue(createdBoard, nameof(Board.Id), newBoardId);
        
        boardServiceMock
            .Setup(s => s.CreateBoardAsync(
                command.Name, 
                command.Description,
                command.ProjectId,
                userId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBoard);

        var handler = new CreateBoardCommandHandler(currentUserServiceMock.Object, boardServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(newBoardId, response.Result.Value);
        Assert.Equal("Board created successfully", response.Result.Message);

        boardServiceMock.Verify(s => s.CreateBoardAsync(command.Name, command.Description,command.ProjectId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateBoardCommand { ProjectId = 1, Name = "Test Board" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardServiceMock = new Mock<IBoardService>();
        var handler = new CreateBoardCommandHandler(currentUserServiceMock.Object, boardServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);

        boardServiceMock.Verify(s => s.CreateBoardAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldPassZeroAsCreatorId()
    {
        // Arrange
        var newBoardId = 789L;
        var command = new CreateBoardCommand { ProjectId = 2, Name = "Another Board" };
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardServiceMock = new Mock<IBoardService>();
        var createdBoard = (Board)FormatterServices.GetUninitializedObject(typeof(Board));
        SetPrivatePropertyValue(createdBoard, nameof(Board.Id), newBoardId);
        
        boardServiceMock
            .Setup(s => s.CreateBoardAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<long>(), 
                0, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBoard);

        var handler = new CreateBoardCommandHandler(currentUserServiceMock.Object, boardServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(newBoardId, response.Result.Value);

        boardServiceMock.Verify(s => s.CreateBoardAsync(command.Name, command.Description, command.ProjectId, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}