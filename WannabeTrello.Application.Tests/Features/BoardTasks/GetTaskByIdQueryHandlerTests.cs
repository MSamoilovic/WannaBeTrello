using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.GetTaskById;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Appilcation.Tests.Features.BoardTasks;

public class GetTaskByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndTaskExists_ShouldReturnTaskResponse()
    {
        // Arrange
        var userId = 123L;
        var taskId = 1L;
        var query = new GetTaskByIdQuery(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var taskFromService = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.SetPrivatePropertyValue(taskFromService, nameof(BoardTask.Id), taskId);
        
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskFromService);

        var handler = new GetTaskByIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(taskId, response.Id);
        taskServiceMock.Verify(s => s.GetTaskByIdAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetTaskByIdQuery(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var taskServiceMock = new Mock<IBoardTaskService>();
        var handler = new GetTaskByIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(query, CancellationToken.None));
            
        Assert.Equal("User is not authenticated", exception.Message);
        taskServiceMock.Verify(s => s.GetTaskByIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFoundException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentTaskId = 999L;
        var query = new GetTaskByIdQuery(nonExistentTaskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(nonExistentTaskId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(BoardTask), nonExistentTaskId));

        var handler = new GetTaskByIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenServiceThrowsAccessDeniedException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var taskIdWithNoAccess = 403L;
        var query = new GetTaskByIdQuery(taskIdWithNoAccess);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var taskServiceMock = new Mock<IBoardTaskService>();
        taskServiceMock
            .Setup(s => s.GetTaskByIdAsync(taskIdWithNoAccess, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Access denied."));

        var handler = new GetTaskByIdQueryHandler(taskServiceMock.Object, currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
}