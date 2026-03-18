using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Tasks.GetCommentsByTaskId;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.BoardTasks;

public class GetCommentsByTaskIdCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndTaskHasComments_ShouldReturnCommentsList()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();

        // Kreiranje test komentara
        var comment1 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.Content), "First comment");
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.TaskId), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.UserId), 10L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.IsDeleted), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.IsEdited), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.EditedAt), null);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.CreatedAt), DateTime.UtcNow.AddHours(-2));
        ApplicationTestUtils.InitializeDomainEvents(comment1);

        var comment2 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.Id), 2L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.Content), "Second comment");
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.TaskId), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.UserId), 20L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.IsDeleted), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.IsEdited), true);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.EditedAt), DateTime.UtcNow.AddMinutes(-15));
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.CreatedAt), DateTime.UtcNow.AddHours(-1));
        ApplicationTestUtils.InitializeDomainEvents(comment2);

        var comment3 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.Id), 3L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.Content), "Third comment");
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.TaskId), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.IsDeleted), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.IsEdited), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.EditedAt), null);
        ApplicationTestUtils.SetPrivatePropertyValue(comment3, nameof(Comment.CreatedAt), DateTime.UtcNow.AddMinutes(-30));
        ApplicationTestUtils.InitializeDomainEvents(comment3);

        var commentsFromService = new List<Comment> { comment1, comment2, comment3 };

        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(commentsFromService);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Count);

        // Verify first comment
        Assert.Equal(1L, response[0].Id);
        Assert.Equal("First comment", response[0].Content);
        Assert.Equal(taskId, response[0].TaskId);
        Assert.Equal(10L, response[0].UserId);
        Assert.False(response[0].IsDeleted);
        Assert.False(response[0].IsEdited);
        Assert.Null(response[0].EditedAt);

        // Verify second comment (edited)
        Assert.Equal(2L, response[1].Id);
        Assert.Equal("Second comment", response[1].Content);
        Assert.Equal(taskId, response[1].TaskId);
        Assert.Equal(20L, response[1].UserId);
        Assert.False(response[1].IsDeleted);
        Assert.True(response[1].IsEdited);
        Assert.NotNull(response[1].EditedAt);

        // Verify third comment (by current user)
        Assert.Equal(3L, response[2].Id);
        Assert.Equal("Third comment", response[2].Content);
        Assert.Equal(userId, response[2].UserId);

        commentServiceMock.Verify(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        commentServiceMock.Verify(s => s.GetCommentsByTaskId(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IEnumerable<Comment>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        commentServiceMock.Verify(s => s.GetCommentsByTaskId(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IEnumerable<Comment>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTaskHasNoComments_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
        commentServiceMock.Verify(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentTaskId = 999L;
        var command = new GetCommentsByTaskIdCommand(nonExistentTaskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(nonExistentTaskId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(BoardTask), nonExistentTaskId));

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Entity 'BoardTask' ({nonExistentTaskId}) was not found.", exception.Message);
        commentServiceMock.Verify(s => s.GetCommentsByTaskId(nonExistentTaskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotHaveAccessToTask_ShouldPropagateAccessDeniedException()
    {
        // Arrange
        var userId = 123L;
        var taskIdWithNoAccess = 403L;
        var command = new GetCommentsByTaskIdCommand(taskIdWithNoAccess);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskIdWithNoAccess, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("You don't have permission to view comments for this task."));

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("You don't have permission to view comments for this task.", exception.Message);
        commentServiceMock.Verify(s => s.GetCommentsByTaskId(taskIdWithNoAccess, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapCommentPropertiesCorrectly()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);
        var editedAt = DateTime.UtcNow.AddMinutes(-10);
        var createdAt = DateTime.UtcNow.AddHours(-1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), 100L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Content), "Test content for mapping");
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), taskId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.UserId), 50L);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.IsDeleted), false);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.IsEdited), true);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.EditedAt), editedAt);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.CreatedAt), createdAt);
        ApplicationTestUtils.InitializeDomainEvents(comment);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment> { comment });

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(response);
        var mappedComment = response[0];
        
        Assert.Equal(100L, mappedComment.Id);
        Assert.Equal("Test content for mapping", mappedComment.Content);
        Assert.Equal(taskId, mappedComment.TaskId);
        Assert.Equal(50L, mappedComment.UserId);
        Assert.False(mappedComment.IsDeleted);
        Assert.True(mappedComment.IsEdited);
        Assert.Equal(editedAt, mappedComment.EditedAt);
    }

    [Fact]
    public async Task Handle_WithMultipleComments_ShouldPreserveOrderFromService()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        // Create comments in specific order
        var comments = new List<Comment>();
        for (int i = 1; i <= 5; i++)
        {
            var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), (long)i);
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Content), $"Comment {i}");
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), taskId);
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.UserId), (long)(i * 10));
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.IsDeleted), false);
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.IsEdited), false);
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.EditedAt), null);
            ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.CreatedAt), DateTime.UtcNow.AddMinutes(-i));
            ApplicationTestUtils.InitializeDomainEvents(comment);
            comments.Add(comment);
        }

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, response.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i + 1, response[i].Id);
            Assert.Equal($"Comment {i + 1}", response[i].Content);
            Assert.Equal((i + 1) * 10, response[i].UserId);
        }
    }
    
    [Fact]
    public async Task Handle_WhenCommentsAreCached_ShouldUseCacheKey()
    {
        // Arrange
        var userId = 123L;
        var taskId = 456L;
        var command = new GetCommentsByTaskIdCommand(taskId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.GetCommentsByTaskId(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<IReadOnlyList<Comment>>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetCommentsByTaskIdCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            $"task:{taskId}:comments",
            It.IsAny<Func<Task<IReadOnlyList<Comment>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

