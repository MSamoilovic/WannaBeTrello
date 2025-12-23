using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.GetTasksByBoardId;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class GetTasksByBoardIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndBoardExists_ShouldReturnTasksList()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardId };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        
        // Kreiranje test taskova
        var task1 = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.Title), "Task 1");
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.Description), "Description 1");
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.Priority), TaskPriority.High);
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.DueDate), DateTime.Now.AddDays(5));
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.ColumnId), 10L);
        ApplicationTestUtils.SetPrivatePropertyValue(task1, nameof(BoardTask.AssigneeId), 200L);
        ApplicationTestUtils.InitializeDomainEvents(task1);
        
        var task2 = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.Id), 2L);
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.Title), "Task 2");
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.Description), "Description 2");
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.Priority), TaskPriority.Medium);
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.DueDate), DateTime.Now.AddDays(10));
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.ColumnId), 11L);
        ApplicationTestUtils.SetPrivatePropertyValue(task2, nameof(BoardTask.AssigneeId), null);
        ApplicationTestUtils.InitializeDomainEvents(task2);
        
        var tasksFromService = new List<BoardTask> { task1, task2 };

        taskServiceMock
            .Setup(s => s.GetTasksByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasksFromService);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(1L, response[0].Id);
        Assert.Equal("Task 1", response[0].Title);
        Assert.Equal("Description 1", response[0].Description);
        Assert.Equal(TaskPriority.High, response[0].Priority);
        Assert.Equal(10L, response[0].ColumnId);
        Assert.Equal(200L, response[0].AssigneeId);
        
        Assert.Equal(2L, response[1].Id);
        Assert.Equal("Task 2", response[1].Title);
        Assert.Null(response[1].AssigneeId);
        
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowAccessDeniedException()
    {
        // Arrange
        var boardId = 1L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardId };
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IEnumerable<BoardTask>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowAccessDeniedException()
    {
        // Arrange
        var boardId = 1L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardId };
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IEnumerable<BoardTask>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenBoardDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentBoardId = 999L;
        var query = new GetTasksByBoardIdQuery { BoardId = nonExistentBoardId };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTasksByBoardIdAsync(nonExistentBoardId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Board), nonExistentBoardId));

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal($"Entity 'Board' ({nonExistentBoardId}) was not found.", exception.Message);
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(nonExistentBoardId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenUserDoesNotHaveAccessToBoard_ShouldPropagateAccessDeniedException()
    {
        // Arrange
        var userId = 123L;
        var boardIdWithNoAccess = 403L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardIdWithNoAccess };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTasksByBoardIdAsync(boardIdWithNoAccess, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("You don't have permission to view this board."));

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("You don't have permission to view this board.", exception.Message);
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(boardIdWithNoAccess, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenBoardHasNoTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardId };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTasksByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BoardTask>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        taskServiceMock.Verify(s => s.GetTasksByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenTasksAreCached_ShouldUseCacheKey()
    {
        // Arrange
        var userId = 123L;
        var boardId = 456L;
        var query = new GetTasksByBoardIdQuery { BoardId = boardId };

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTasksByBoardIdAsync(boardId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BoardTask>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<BoardTask>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetTasksByBoardIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            $"board:{boardId}:tasks",
            It.IsAny<Func<Task<IReadOnlyList<BoardTask>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

