using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Tasks.ClearTaskRecurrence;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Features.BoardTasks;

public class ClearTaskRecurrenceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBoardTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public ClearTaskRecurrenceCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);
    }

    private ClearTaskRecurrenceCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);

    private static BoardTask BuildRecurringTask(long id)
    {
        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), id);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Title), "Task");
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Recurrence),
            RecurrenceRule.Create(RecurrenceFrequency.Daily));
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.NextOccurrence), DateTime.UtcNow.Date.AddDays(1));
        ApplicationTestUtils.InitializeDomainEvents(task);
        typeof(BoardTask)
            .GetField("_activities", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(task, new List<Activity>());
        return task;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldClearRecurrenceAndPersist()
    {
        const long userId = 10L;
        const long taskId = 5L;
        var task = BuildRecurringTask(taskId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _taskRepositoryMock
            .Setup(r => r.GetTaskDetailsByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var response = await CreateHandler().Handle(new ClearTaskRecurrenceCommand(taskId), CancellationToken.None);

        Assert.True(response.Result.IsSuccess);
        Assert.Null(task.Recurrence);
        Assert.Null(task.NextOccurrence);
        Assert.False(task.IsRecurring);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new ClearTaskRecurrenceCommand(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _taskRepositoryMock
            .Setup(r => r.GetTaskDetailsByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardTask?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new ClearTaskRecurrenceCommand(99L), CancellationToken.None));
    }
}
