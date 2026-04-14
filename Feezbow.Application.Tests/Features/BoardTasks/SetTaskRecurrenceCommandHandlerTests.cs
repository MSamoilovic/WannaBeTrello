using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Tasks.SetTaskRecurrence;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.ValueObjects;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.BoardTasks;

public class SetTaskRecurrenceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBoardTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public SetTaskRecurrenceCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);
    }

    private SetTaskRecurrenceCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);

    private static BoardTask BuildTask(long id, DateTime? dueDate)
    {
        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), id);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Title), "Task");
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.DueDate), dueDate);
        ApplicationTestUtils.InitializeDomainEvents(task);
        typeof(BoardTask)
            .GetField("_activities", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(task, new List<Activity>());
        return task;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldSetRecurrenceAndPersist()
    {
        const long userId = 10L;
        const long taskId = 5L;
        var dueDate = DateTime.UtcNow.Date.AddDays(1);
        var task = BuildTask(taskId, dueDate);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _taskRepositoryMock
            .Setup(r => r.GetTaskDetailsByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new SetTaskRecurrenceCommand(taskId, RecurrenceFrequency.Daily, 1, null, null);

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.True(task.IsRecurring);
        Assert.NotNull(task.Recurrence);
        Assert.Equal(RecurrenceFrequency.Daily, task.Recurrence!.Frequency);
        Assert.Equal(dueDate.AddDays(1), task.NextOccurrence);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new SetTaskRecurrenceCommand(1L, RecurrenceFrequency.Daily, 1, null, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _taskRepositoryMock
            .Setup(r => r.GetTaskDetailsByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardTask?)null);

        var command = new SetTaskRecurrenceCommand(99L, RecurrenceFrequency.Daily, 1, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }
}
