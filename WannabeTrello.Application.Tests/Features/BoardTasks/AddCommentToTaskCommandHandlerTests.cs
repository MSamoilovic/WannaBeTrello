using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.AddCommentToTask;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class AddCommentToTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldAddCommentAndReturnId()
    {
        // Arrange
        var command = new AddCommentToTaskCommand
        {
            TaskId = 55L,
            Content = "New comment"
        };

        var currentUserId = 5L;
        var createdCommentId = 999L;

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        boardTaskServiceMock
            .Setup(s => s.AddCommentToTaskAsync(command.TaskId, currentUserId, command.Content))
            .ReturnsAsync(createdCommentId);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new AddCommentToTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(createdCommentId, result);
        boardTaskServiceMock.Verify(
            s => s.AddCommentToTaskAsync(command.TaskId, currentUserId, command.Content),
            Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.TaskComments(command.TaskId)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrow()
    {
        // Arrange
        var command = new AddCommentToTaskCommand { TaskId = 55L, Content = "New comment" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new AddCommentToTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Korisnik nije autentifikovan.", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.AddCommentToTaskAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrow()
    {
        // Arrange
        var command = new AddCommentToTaskCommand { TaskId = 55L, Content = "New comment" };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var boardTaskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new AddCommentToTaskCommandHandler(boardTaskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Korisnik nije autentifikovan.", exception.Message);
        boardTaskServiceMock.Verify(
            s => s.AddCommentToTaskAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()),
            Times.Never);
    }
}

