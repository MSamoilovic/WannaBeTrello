using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Columns.UpdateColumn;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Columns;

public class UpdateColumnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldUpdateColumnAndReturnResponse()
    {
        // Arrange
        var userId = 123L;
        var columnId = 1L;
        var command = new UpdateColumnCommand(columnId,"Updated Name", 5 );

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        var updatedColumnFromService = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(updatedColumnFromService, nameof(Column.Id), columnId);
        ApplicationTestUtils.SetPrivatePropertyValue(updatedColumnFromService, nameof(Column.BoardId), 123L);

        columnServiceMock
            .Setup(s => s.UpdateColumnAsync(command.ColumnId!, command.NewName, command.WipLimit, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedColumnFromService);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new UpdateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(columnId, response.Id);
        columnServiceMock.Verify(s => s.UpdateColumnAsync(columnId, command.NewName, command.WipLimit, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Column(columnId)), It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.BoardColumns(123L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateColumnCommand(0, null, null);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var columnServiceMock = new Mock<IColumnService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateColumnCommand(0, null, null);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var columnServiceMock = new Mock<IColumnService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var columnId = 999L;
        var command = new UpdateColumnCommand(columnId, null, null);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        columnServiceMock
            .Setup(s => s.UpdateColumnAsync(columnId, command.NewName, command.WipLimit, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Column), columnId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateColumnCommandHandler(columnServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}