using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Tasks.SearchTasks;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.BoardTasks;

public class SearchTaskQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskServiceMock = new Mock<IBoardTaskService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new SearchTaskQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchTaskQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));

        taskServiceMock.Verify(x => x.SearchTasks(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskServiceMock = new Mock<IBoardTaskService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var handler = new SearchTaskQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchTaskQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));

        taskServiceMock.Verify(x => x.SearchTasks(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnTasksQueryable()
    {
        // Arrange
        var userId = 5L;
        var tasks = new List<BoardTask>().AsQueryable();

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(x => x.SearchTasks(userId))
            .Returns(tasks);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new SearchTaskQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchTaskQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        taskServiceMock.Verify(x => x.SearchTasks(userId), Times.Once);
    }
}
