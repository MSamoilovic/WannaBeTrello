using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Columns.GetColumn;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Columns;

public class GetColumnByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndColumnExists_ShouldReturnColumnResponse()
    {
        // Arrange
        var userId = 123L;
        var columnId = 1L;
        var query = new GetColumnByIdQuery(columnId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        
        var columnFromService = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.SetPrivatePropertyValue(columnFromService, nameof(Column.Id), columnId);
        ApplicationTestUtils.SetPrivatePropertyValue(columnFromService, "_tasks", new List<BoardTask>()); // Inicijalizacija za FromEntity

        columnServiceMock
            .Setup(s => s.GetColumnByIdAsync(columnId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(columnFromService);

        var handler = new GetColumnByIdQueryHandler(columnServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(columnId, response.ColumnId);
        columnServiceMock.Verify(s => s.GetColumnByIdAsync(columnId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetColumnByIdQuery(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null); // UserId je null

        var columnServiceMock = new Mock<IColumnService>();
        var handler = new GetColumnByIdQueryHandler(columnServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        columnServiceMock.Verify(s => s.GetColumnByIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentColumnId = 999L;
        var query = new GetColumnByIdQuery(nonExistentColumnId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var columnServiceMock = new Mock<IColumnService>();
        columnServiceMock
            .Setup(s => s.GetColumnByIdAsync(nonExistentColumnId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Column), nonExistentColumnId));

        var handler = new GetColumnByIdQueryHandler(columnServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
}