using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Tests.Utils;

namespace WannabeTrello.Domain.Tests.Entities;

public class CommentTests
{
    // --- Testovi za Create metodu ---

    [Fact]
    public void Create_WithValidParameters_ShouldCreateCommentInCorrectState()
    {
        // Arrange
        const long taskId = 1L;
        const string content = "This is a test comment.";
        const long userId = 10L;

        // Act
        var comment = Comment.Create(taskId, content, userId);

        // Assert
        Assert.NotNull(comment);
        Assert.Equal(taskId, comment.TaskId);
        Assert.Equal(content, comment.Content);
        Assert.Equal(userId, comment.UserId);
        Assert.False(comment.IsDeleted);
        Assert.False(comment.IsEdited);
        Assert.Null(comment.EditedAt);
        Assert.Equal(userId, comment.CreatedBy);
        Assert.True((DateTime.UtcNow - comment.CreatedAt).TotalSeconds < 1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyContent_ShouldThrowBusinessRuleValidationException(string invalidContent)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            Comment.Create(1L, invalidContent, 10L));

        Assert.Equal("Comment content cannot be empty.", exception.Message);
    }

    [Fact]
    public void Create_WithContentLongerThan300Characters_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var longContent = new string('a', 301);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            Comment.Create(1L, longContent, 10L));

        Assert.Equal("Comment content cannot be longer than 300 characters.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidTaskId_ShouldThrowBusinessRuleValidationException(long invalidTaskId)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            Comment.Create(invalidTaskId, "Valid content", 10L));

        Assert.Equal("TaskId must be a positive number.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidUserId_ShouldThrowBusinessRuleValidationException(long invalidUserId)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            Comment.Create(1L, "Valid content", invalidUserId));

        Assert.Equal("Comment must have a valid author.", exception.Message);
    }

    // --- Testovi za UpdateContent metodu ---

    [Fact]
    public void UpdateContent_WithValidContent_ShouldUpdateContentAndSetProperties()
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);
        DomainTestUtils.InitializeDomainEvents(comment); // Reset events after Create
        const long modifyingUserId = 20L;
        const string newContent = "Updated content";

        // Act
        comment.UpdateContent(newContent, modifyingUserId);

        // Assert
        Assert.Equal(newContent, comment.Content);
        Assert.True(comment.IsEdited);
        Assert.NotNull(comment.EditedAt);
        Assert.True((DateTime.UtcNow - comment.EditedAt.Value).TotalSeconds < 1);
        Assert.NotNull(comment.LastModifiedAt);
        Assert.Equal(modifyingUserId, comment.LastModifiedBy);
    }

    [Fact]
    public void UpdateContent_WithValidContent_ShouldRaiseCommentUpdatedEvent()
    {
        // Arrange
       
        const string oldContent = "Original content";
        const string newContent = "Updated content";
        var comment = Comment.Create(1L, oldContent, 10L);
        DomainTestUtils.InitializeDomainEvents(comment);
        const long modifyingUserId = 20L;

        // Act
        comment.UpdateContent(newContent, modifyingUserId);

        // Assert
        var domainEvent = Assert.Single(comment.DomainEvents);
        var updatedEvent = Assert.IsType<CommentUpdatedEvent>(domainEvent);
        Assert.Equal(comment.Id, updatedEvent.CommentId);
        Assert.Equal(comment.TaskId, updatedEvent.TaskId);
        Assert.Equal(oldContent, updatedEvent.OldContent["Content"]);
        Assert.Equal(newContent, updatedEvent.NewContent["Content"]);
        Assert.Equal(modifyingUserId, updatedEvent.ModifyingUserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateContent_WithEmptyContent_ShouldThrowBusinessRuleValidationException(string invalidContent)
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            comment.UpdateContent(invalidContent, 20L));

        Assert.Equal("Comment content cannot be empty.", exception.Message);
    }

    [Fact]
    public void UpdateContent_WithContentLongerThan300Characters_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);
        var longContent = new string('a', 301);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            comment.UpdateContent(longContent, 20L));

        Assert.Equal("Comment content cannot be longer than 300 characters.", exception.Message);
    }

    [Fact]
    public void UpdateContent_OnDeletedComment_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);
        comment.Delete(10L);

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            comment.UpdateContent("New content", 20L));

        Assert.Equal("Comment is deleted.", exception.Message);
    }

    // --- Testovi za Delete metodu ---

    [Fact]
    public void Delete_ValidComment_ShouldMarkAsDeletedAndRaiseEvent()
    {
        // Arrange
        var comment = Comment.Create(1L, "Test content", 10L);
        DomainTestUtils.InitializeDomainEvents(comment);
        const long modifierUserId = 20L;

        // Act
        comment.Delete(modifierUserId);

        // Assert
        Assert.True(comment.IsDeleted);
        Assert.NotNull(comment.LastModifiedAt);
        Assert.Equal(modifierUserId, comment.LastModifiedBy);

        var domainEvent = Assert.Single(comment.DomainEvents);
        var deletedEvent = Assert.IsType<CommentDeletedEvent>(domainEvent);
        Assert.Equal(comment.Id, deletedEvent.CommentId);
        Assert.Equal(comment.TaskId, deletedEvent.TaskId);
        Assert.Equal(modifierUserId, deletedEvent.ModifyingUserId);
    }

    [Fact]
    public void Delete_AlreadyDeletedComment_ShouldDoNothing()
    {
        // Arrange
        var comment = Comment.Create(1L, "Test content", 10L);
        comment.Delete(10L);
        DomainTestUtils.InitializeDomainEvents(comment); // Clear events after first delete

        // Act
        comment.Delete(20L); // Try to delete again

        // Assert
        Assert.True(comment.IsDeleted);
        Assert.Empty(comment.DomainEvents); // No new event should be raised
    }

    // --- Testovi za Restore metodu ---

    [Fact]
    public void Restore_DeletedComment_ShouldRestoreAndRaiseEvent()
    {
        // Arrange
        var comment = Comment.Create(1L, "Test content", 10L);
        comment.Delete(10L);
        DomainTestUtils.InitializeDomainEvents(comment); // Clear events after delete
        const long modifierUserId = 30L;

        // Act
        comment.Restore(modifierUserId);

        // Assert
        Assert.False(comment.IsDeleted);
        Assert.NotNull(comment.LastModifiedAt);
        Assert.Equal(modifierUserId, comment.LastModifiedBy);

        var domainEvent = Assert.Single(comment.DomainEvents);
        var restoredEvent = Assert.IsType<CommentRestoredEvent>(domainEvent);
        Assert.Equal(comment.Id, restoredEvent.CommentId);
        Assert.Equal(comment.TaskId, restoredEvent.TaskId);
        Assert.Equal(modifierUserId, restoredEvent.ModifyingUserId);
    }

    [Fact]
    public void Restore_NotDeletedComment_ShouldDoNothing()
    {
        // Arrange
        var comment = Comment.Create(1L, "Test content", 10L);
        DomainTestUtils.InitializeDomainEvents(comment); // Clear events after create

        // Act
        comment.Restore(20L);

        // Assert
        Assert.False(comment.IsDeleted);
        Assert.Empty(comment.DomainEvents); // No event should be raised
    }

    // --- Integration testovi ---

    [Fact]
    public void Comment_MultipleUpdates_ShouldTrackEditStateCorrectly()
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);
        
        // Act - First update
        comment.UpdateContent("First update", 10L);
        var firstEditTime = comment.EditedAt;
        
        // Wait a bit to ensure time difference
        System.Threading.Thread.Sleep(10);
        
        // Act - Second update
        comment.UpdateContent("Second update", 10L);
        var secondEditTime = comment.EditedAt;

        // Assert
        Assert.True(comment.IsEdited);
        Assert.Equal("Second update", comment.Content);
        Assert.NotNull(secondEditTime);
        Assert.True(secondEditTime > firstEditTime);
    }

    [Fact]
    public void Comment_DeleteAndRestore_ShouldWorkCorrectly()
    {
        // Arrange
        var comment = Comment.Create(1L, "Test content", 10L);
        
        // Act - Delete
        comment.Delete(20L);
        Assert.True(comment.IsDeleted);
        
        // Act - Restore
        comment.Restore(30L);
        
        // Assert
        Assert.False(comment.IsDeleted);
        Assert.NotNull(comment.LastModifiedAt);
        Assert.Equal(30L, comment.LastModifiedBy);
    }

    [Fact]
    public void Comment_UpdateThenDelete_ShouldPreventFurtherUpdates()
    {
        // Arrange
        var comment = Comment.Create(1L, "Original content", 10L);
        comment.UpdateContent("Updated content", 20L);
        
        // Act - Delete
        comment.Delete(30L);
        
        // Assert - Try to update deleted comment
        Assert.Throws<BusinessRuleValidationException>(() =>
            comment.UpdateContent("This should fail", 40L));
    }
}

