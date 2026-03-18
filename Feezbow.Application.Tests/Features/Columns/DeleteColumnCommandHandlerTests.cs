using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Columns.DeleteColumn;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Columns;

public class DeleteColumnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldDeleteColumnAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var columnId = 456L;
        var command = new DeleteColumnCommand(columnId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        
        // Column sa BoardId (za invalidaciju keša)
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.Id), columnId);
        ApplicationTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), 123L);
        
        columnServiceMock
            .Setup(s => s.GetColumnByIdAsync(columnId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(column);
        
        columnServiceMock
            .Setup(s => s.DeleteColumnAsync(columnId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(columnId); // Vraća ID uspešno obrisane kolone

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new DeleteColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(columnId, response.Result.Value);
        Assert.Equal("Column deleted successfully", response.Result.Message);

        columnServiceMock.Verify(s => s.DeleteColumnAsync(columnId, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Column(columnId)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardColumns(123L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new DeleteColumnCommand(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var columnServiceMock = new Mock<IColumnService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        columnServiceMock.Verify(s => s.DeleteColumnAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var columnId = 789L;
        var command = new DeleteColumnCommand(columnId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var columnServiceMock = new Mock<IColumnService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        columnServiceMock.Verify(s => s.DeleteColumnAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentColumnId = 999L;
        var command = new DeleteColumnCommand(nonExistentColumnId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        columnServiceMock
            .Setup(s => s.DeleteColumnAsync(nonExistentColumnId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Domain.Entities.Column), nonExistentColumnId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
    }
}