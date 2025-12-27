using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Columns.CreateColumn;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Columns;

public class CreateColumnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldCreateColumnAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var newColumnId = 789L;
        var command = new CreateColumnCommand
        {
            BoardId = 1,
            Name = "New Column",
            Order = 1
        };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        var createdColumn = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(createdColumn, nameof(Column.Id), newColumnId);

        columnServiceMock
            .Setup(s => s.CreateColumnAsync(
                command.BoardId,
                command.Name,
                command.Order,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdColumn);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new CreateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(newColumnId, response.Result.Value);
        Assert.Equal("Column created successfully", response.Result.Message);

        columnServiceMock.Verify(s => s.CreateColumnAsync(command.BoardId, command.Name, command.Order, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardColumns(command.BoardId)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateColumnCommand();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var columnServiceMock = new Mock<IColumnService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new CreateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);

        columnServiceMock.Verify(s => s.CreateColumnAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldCallServiceWithZeroAsCreatorId()
    {
        // Arrange
        var newColumnId = 456L;
        var command = new CreateColumnCommand { BoardId = 2, Name = "Another Column", Order = 2 };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null); // UserId je null

        var columnServiceMock = new Mock<IColumnService>();
        var createdColumn = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(createdColumn, nameof(Column.Id), newColumnId);

        columnServiceMock
            .Setup(s => s.CreateColumnAsync(
                command.BoardId,
                command.Name,
                command.Order,
                0, // Očekujemo da se prosledi 0
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdColumn);
        
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new CreateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        
        columnServiceMock.Verify(s => s.CreateColumnAsync(command.BoardId, command.Name, command.Order, 0, It.IsAny<CancellationToken>()), Times.Once);
    }
}