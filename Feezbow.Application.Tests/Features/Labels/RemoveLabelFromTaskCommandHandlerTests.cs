using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Labels.RemoveLabelFromTask;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Labels;

public class RemoveLabelFromTaskCommandHandlerTests
{
    private readonly Mock<IBoardTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private RemoveLabelFromTaskCommandHandler CreateHandler() => new(
        _taskRepositoryMock.Object,
        _currentUserServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenLabelIsOnTask_ShouldRemoveLabelAndInvalidateCache()
    {
        // Arrange
        var userId = 1L;
        var taskId = 5L;
        var labelId = 10L;
        var boardId = 20L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), taskId);

        var existingTaskLabel = new BoardTaskLabel { TaskId = taskId, LabelId = labelId };
        ApplicationTestUtils.SetPrivatePropertyValue(task, "_taskLabels", new List<BoardTaskLabel> { existingTaskLabel });

        _taskRepositoryMock.Setup(r => r.GetTaskWithLabelsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var response = await CreateHandler().Handle(new RemoveLabelFromTaskCommand(taskId, labelId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(task.TaskLabels);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.Task(taskId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new RemoveLabelFromTaskCommand(1L, 1L), CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _taskRepositoryMock.Setup(r => r.GetTaskWithLabelsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardTask?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateHandler().Handle(new RemoveLabelFromTaskCommand(99L, 1L), CancellationToken.None));
    }
}
