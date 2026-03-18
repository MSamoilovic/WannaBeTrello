using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Comments.UpdateCommentContent;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Comments;

public class UpdateCommentContentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndOwnsComment_ShouldUpdateCommentAndReturnSuccessResponse()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var newContent = "Updated comment content";
        var command = new UpdateCommentContentCommand(commentId, newContent);

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
            .Setup(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(commentId, response.Result.Value);
        Assert.Equal("Comment updated successfully", response.Result.Message);

        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.TaskComments(789L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateCommentContentCommand(456L, "New content");

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateCommentContentCommand(456L, "New content");

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var commentServiceMock = new Mock<ICommentService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User is not authenticated.", exception.Message);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        var userId = 123L;
        var nonExistentCommentId = 999L;
        var newContent = "New content";
        var command = new UpdateCommentContentCommand(nonExistentCommentId, newContent);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(nonExistentCommentId, newContent, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Comment), nonExistentCommentId));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal($"Entity 'Comment' ({nonExistentCommentId}) was not found.", exception.Message);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(nonExistentCommentId, newContent, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnComment_ShouldPropagateAccessDeniedException()
    {
        // Arrange
        var userId = 123L; // Current user
        var commentId = 456L; // Comment owned by someone else
        var newContent = "New content";
        var command = new UpdateCommentContentCommand(commentId, newContent);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Samo autor komentara može menjati komentar."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Samo autor komentara može menjati komentar.", exception.Message);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPassCorrectParametersToService()
    {
        // Arrange
        var userId = 777L;
        var commentId = 888L;
        var newContent = "This is updated content";
        var command = new UpdateCommentContentCommand(commentId, newContent);
        var cancellationToken = new CancellationToken();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment, nameof(Comment.TaskId), 789L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId, cancellationToken))
            .ReturnsAsync(comment);
        
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(commentId, newContent, userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert - Verify exact parameters
        commentServiceMock.Verify(
            s => s.UpdateCommentAsync(commentId, newContent, userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCommentIsDeleted_ShouldPropagateBusinessRuleValidationException()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var newContent = "New content";
        var command = new UpdateCommentContentCommand(commentId, newContent);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Comment is deleted."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Comment is deleted.", exception.Message);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, newContent, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyContent_ShouldPropagateBusinessRuleValidationException()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var emptyContent = "";
        var command = new UpdateCommentContentCommand(commentId, emptyContent);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(commentId, emptyContent, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Comment content cannot be empty."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Comment content cannot be empty.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithLongContent_ShouldPropagateBusinessRuleValidationException()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var longContent = new string('a', 301); // > 300 characters
        var command = new UpdateCommentContentCommand(commentId, longContent);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(commentId, longContent, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleValidationException("Comment content cannot be longer than 300 characters."));

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("Comment content cannot be longer than 300 characters.", exception.Message);
    }

    [Fact]
    public async Task Handle_MultipleCallsWithSameComment_ShouldCallServiceEachTime()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        var content1 = "First update";
        var content2 = "Second update";
        var command1 = new UpdateCommentContentCommand(commentId, content1);
        var command2 = new UpdateCommentContentCommand(commentId, content2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comment sa TaskId (za invalidaciju keša)
        var comment1 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.TaskId), 789L);
        
        var comment2 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.Id), commentId);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.TaskId), 789L);
        
        commentServiceMock
            .SetupSequence(s => s.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment1)
            .ReturnsAsync(comment2);
        
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(It.IsAny<long>(), It.IsAny<string>(), userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        await handler.Handle(command1, CancellationToken.None);
        await handler.Handle(command2, CancellationToken.None);

        // Assert
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, content1, userId, It.IsAny<CancellationToken>()), Times.Once);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, content2, userId, It.IsAny<CancellationToken>()), Times.Once);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(It.IsAny<long>(), It.IsAny<string>(), userId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithDifferentComments_ShouldCallServiceWithCorrectIds()
    {
        // Arrange
        var userId = 123L;
        var commentId1 = 456L;
        var commentId2 = 789L;
        var content1 = "Content for comment 1";
        var content2 = "Content for comment 2";
        var command1 = new UpdateCommentContentCommand(commentId1, content1);
        var command2 = new UpdateCommentContentCommand(commentId2, content2);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var commentServiceMock = new Mock<ICommentService>();
        
        // Comments sa TaskId (za invalidaciju keša)
        var comment1 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.Id), commentId1);
        ApplicationTestUtils.SetPrivatePropertyValue(comment1, nameof(Comment.TaskId), 789L);
        
        var comment2 = ApplicationTestUtils.CreateInstanceWithoutConstructor<Comment>();
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.Id), commentId2);
        ApplicationTestUtils.SetPrivatePropertyValue(comment2, nameof(Comment.TaskId), 790L);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment1);
        
        commentServiceMock
            .Setup(s => s.GetCommentByIdAsync(commentId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment2);
        
        commentServiceMock
            .Setup(s => s.UpdateCommentAsync(It.IsAny<long>(), It.IsAny<string>(), userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response1 = await handler.Handle(command1, CancellationToken.None);
        var response2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.Equal(commentId1, response1.Result.Value);
        Assert.Equal(commentId2, response2.Result.Value);

        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId1, content1, userId, It.IsAny<CancellationToken>()), Times.Once);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId2, content2, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullContent_ShouldPassNullToService()
    {
        // Arrange
        var userId = 123L;
        var commentId = 456L;
        string? nullContent = null;
        var command = new UpdateCommentContentCommand(commentId, nullContent);

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
            .Setup(s => s.UpdateCommentAsync(commentId, nullContent, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new UpdateCommentContentCommandHandler(commentServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        commentServiceMock.Verify(s => s.UpdateCommentAsync(commentId, null, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

