using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Labels.AddLabelToTask;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Tests.Features.Labels;

public class AddLabelToTaskCommandHandlerTests
{
    private readonly Mock<IBoardTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ILabelRepository> _labelRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private AddLabelToTaskCommandHandler CreateHandler() => new(
        _taskRepositoryMock.Object,
        _labelRepositoryMock.Object,
        _currentUserServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object);

    private static BoardTask CreateTaskWithEmptyLabels(long taskId)
    {
        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "_taskLabels", new List<BoardTaskLabel>());
        return task;
    }

    [Fact]
    public async Task Handle_WhenValidInput_ShouldAddLabelToTask()
    {
        // Arrange
        var userId = 1L;
        var taskId = 5L;
        var labelId = 10L;
        var boardId = 20L;
        var command = new AddLabelToTaskCommand(taskId, labelId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var task = CreateTaskWithEmptyLabels(taskId);
        var label = Label.Create("Bug", "#FF0000", boardId, userId);
        ApplicationTestUtils.SetPrivatePropertyValue(label, nameof(Label.Id), labelId);

        _taskRepositoryMock.Setup(r => r.GetTaskWithLabelsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _labelRepositoryMock.Setup(r => r.GetByIdAsync(labelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);
        _taskRepositoryMock.Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(boardId);

        // Act
        var response = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.Contains("Bug", response.Message);
        Assert.Single(task.TaskLabels);

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
            CreateHandler().Handle(new AddLabelToTaskCommand(1L, 1L), CancellationToken.None));

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
            CreateHandler().Handle(new AddLabelToTaskCommand(99L, 1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenLabelNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var taskId = 5L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _taskRepositoryMock.Setup(r => r.GetTaskWithLabelsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTaskWithEmptyLabels(taskId));
        _labelRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Label?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateHandler().Handle(new AddLabelToTaskCommand(taskId, 99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenLabelBelongsToDifferentBoard_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var userId = 1L;
        var taskId = 5L;
        var labelId = 10L;
        var taskBoardId = 20L;
        var differentBoardId = 99L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var task = CreateTaskWithEmptyLabels(taskId);
        var label = Label.Create("Bug", "#FF0000", differentBoardId, userId);
        ApplicationTestUtils.SetPrivatePropertyValue(label, nameof(Label.Id), labelId);

        _taskRepositoryMock.Setup(r => r.GetTaskWithLabelsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _labelRepositoryMock.Setup(r => r.GetByIdAsync(labelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);
        _taskRepositoryMock.Setup(r => r.GetBoardIdByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskBoardId);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            CreateHandler().Handle(new AddLabelToTaskCommand(taskId, labelId), CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
