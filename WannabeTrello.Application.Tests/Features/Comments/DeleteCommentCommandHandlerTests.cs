using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Comments.DeleteComment;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Comments;

public class DeleteCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndOwnsComment_ShouldDeleteCommentAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var command = new DeleteCommentCommand(commentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), 789L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(commentId, response.Result.Value);
        Assert.Equal("Comment deleted successfully", response.Result.Message);

        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.TaskComments(789L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new DeleteCommentCommand(456L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new DeleteCommentCommand(456L);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated", exception.Message);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentCommentId = 999L;
        var command = new DeleteCommentCommand(nonExistentCommentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(nonExistentCommentId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Comment), nonExistentCommentId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Entity 'Comment' ({nonExistentCommentId}) was not found.", exception.Message);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(nonExistentCommentId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnComment_ShouldPropagateAccessDeniedException()
    {
        // Arrange
        var userId = 123L; // Current user
        var commentId = 456L; // Comment owned by someone else
        var command = new DeleteCommentCommand(commentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Samo autor komentara može obrisati komentar."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Samo autor komentara može obrisati komentar.", exception.Message);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCommentIsAlreadyDeleted_ShouldStillSucceed()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var command = new DeleteCommentCommand(commentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), 789L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        
        // Service ne baca exception (idempotentnost u entity metodi)
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(commentId, response.Result.Value);

        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPassCorrectParametersToService()
    {
        // Arrange
        var userId = 777L;
        var commentId = 888L;
        var command = new DeleteCommentCommand(commentId);
        var cancellationToken = new CancellationToken();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), 999L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert - Verify exact parameters
        commentServiceMock.Verify(
            s => s.DeleteCommentAsync(commentId, userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsBusinessRuleValidationException_ShouldPropagateException()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var command = new DeleteCommentCommand(commentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Some business rule violated."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Some business rule violated.", exception.Message);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleCallsWithSameComment_ShouldCallServiceEachTime()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var command = new DeleteCommentCommand(commentId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), 789L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId, userId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithDifferentComments_ShouldCallServiceWithCorrectIds()
    {
        // Arrange
        var userId = 123L;
        var commentId1 = 456L;
        var commentId2 = 790L; // Changed to avoid conflict with TaskId
        var command1 = new DeleteCommentCommand(commentId1);
        var command2 = new DeleteCommentCommand(commentId2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment 1 sa TaskId
        var comment1 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.Id), commentId1);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.TaskId), 1001L);
        
        // Comment 2 sa TaskId
        var comment2 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.Id), commentId2);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.TaskId), 1002L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment1);
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment2);
        
        commentServiceMock
            .Setup(s => s.DeleteCommentAsync(It.IsAny<long>(), userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new DeleteCommentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response1 = await handler.Handle(command1, CancellationToken.None);
        var response2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.Equal(commentId1, response1.Result.Value);
        Assert.Equal(commentId2, response2.Result.Value);

        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId1, userId, It.IsAny<CancellationToken>()), Times.Once);
        commentServiceMock.Verify(s => s.DeleteCommentAsync(commentId2, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

