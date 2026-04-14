using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Tasks.GetRecurringTasks;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Features.BoardTasks;

public class GetRecurringTasksQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBoardTaskRepository> _taskRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    public GetRecurringTasksQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Tasks).Returns(_taskRepositoryMock.Object);
    }

    private GetRecurringTasksQueryHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);

    private static BoardTask BuildRecurringTask(long id, string title)
    {
        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), id);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Title), title);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.TaskType), TaskType.Chore);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Priority), TaskPriority.Medium);
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Recurrence),
            RecurrenceRule.Create(RecurrenceFrequency.Weekly, 1));
        ApplicationTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.NextOccurrence),
            DateTime.UtcNow.Date.AddDays(3));
        return task;
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnMappedRecurringTasks()
    {
        const long boardId = 11L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        var tasks = new List<BoardTask>
        {
            BuildRecurringTask(1L, "Laundry"),
            BuildRecurringTask(2L, "Trash")
        };

        _taskRepositoryMock
            .Setup(r => r.GetRecurringTasksByBoardAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var response = await CreateHandler().Handle(new GetRecurringTasksQuery(boardId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal("Laundry", response[0].Title);
        Assert.Equal(RecurrenceFrequency.Weekly, response[0].Frequency);
        Assert.Equal(TaskType.Chore, response[0].TaskType);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetRecurringTasksQuery(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoRecurringTasks_ShouldReturnEmptyList()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _taskRepositoryMock
            .Setup(r => r.GetRecurringTasksByBoardAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BoardTask>());

        var response = await CreateHandler().Handle(new GetRecurringTasksQuery(1L), CancellationToken.None);

        Assert.Empty(response);
    }
}
