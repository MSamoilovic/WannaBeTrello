using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Tests.Utils;

namespace WannabeTrello.Domain.Tests.Entities;

public class BoardTaskTests
{
    // --- Testovi za Create metodu ---

    [Fact]
    public void Create_WithValidParameters_ShouldCreateTaskInCorrectState()
    {
        // Arrange
        const string title = "Implement Login Page";
        const string description = "Use JWT for authentication.";
        const TaskPriority priority = TaskPriority.High;
        var dueDate = DateTime.UtcNow.AddDays(3);
        const int position = 1;
        const long columnId = 10L;
        const long assigneeId = 20L;
        const long creatorUserId = 101L;

        // Act
        var task = BoardTask.Create(title, description, priority, dueDate, position, columnId, assigneeId, creatorUserId);

        // Assert
        Assert.NotNull(task);
        Assert.Equal(title, task.Title);
        Assert.Equal(description, task.Description);
        Assert.Equal(priority, task.Priority);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(position, task.Position);
        Assert.Equal(columnId, task.ColumnId);
        Assert.Equal(assigneeId, task.AssigneeId);

        var domainEvent = Assert.Single(task.DomainEvents);
        var taskCreatedEvent = Assert.IsType<TaskCreatedEvent>(domainEvent);
        Assert.Equal(creatorUserId, taskCreatedEvent.CreatorUserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidTitle_ShouldThrowBusinessRuleValidationException(string invalidTitle)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            BoardTask.Create(invalidTitle, null, TaskPriority.Low, DateTime.Now, 1, 1, null, 1L));

        Assert.Equal("Task title cannot be empty.", exception.Message);
    }

    // --- Testovi za UpdateDetails metodu ---

    [Fact]
    public void UpdateDetails_WhenValuesChange_ShouldUpdatePropertiesAndRaiseEvent()
    {
        // Arrange
        var task = BoardTask.Create("Old Title", "Old Desc", TaskPriority.Low, DateTime.Now, 1, 1, null, 1L);
        DomainTestUtils.InitializeDomainEvents(task); // Reset events after Create
        var modifierUserId = 123L;

        var newTitle = "New Title";
        var newDescription = "New Desc";
        var newPriority = TaskPriority.High;
        var newDueDate = DateTime.UtcNow.AddDays(5);

        // Act
        task.UpdateDetails(newTitle, newDescription, newPriority, newDueDate, modifierUserId);

        // Assert
        Assert.Equal(newTitle, task.Title);
        Assert.Equal(newDescription, task.Description);
        Assert.Equal(newPriority, task.Priority);
        Assert.Equal(newDueDate, task.DueDate);

        var domainEvent = Assert.Single(task.DomainEvents);
        var updatedEvent = Assert.IsType<TaskUpdatedEvent>(domainEvent);

        Assert.Equal("Old Title", updatedEvent.OldValues["Title"]);
        Assert.Equal(newTitle, updatedEvent.NewValues["Title"]);
        Assert.Equal("Old Desc", updatedEvent.OldValues["Description"]);
        Assert.Equal(TaskPriority.Low, updatedEvent.OldValues["Priority"]);
    }

    [Fact]
    public void UpdateDetails_WhenNothingChanges_ShouldNotRaiseEvent()
    {
        // Arrange
        var title = "Title";
        var description = "Description";
        var priority = TaskPriority.Medium;
        var dueDate = DateTime.UtcNow.AddMinutes(3);

        var task = BoardTask.Create(title, description, priority, dueDate, 1, 1, null, 1L);
        DomainTestUtils.InitializeDomainEvents(task); // Reset events
        var modifierUserId = 123L;

        // Act
        task.UpdateDetails(title, description, priority, dueDate, modifierUserId);

        // Assert
        Assert.Empty(task.DomainEvents);
    }

    // --- Testovi za MoveToColumn metodu ---

    [Fact]
    public void MoveToColumn_ToDifferentColumn_ShouldChangeColumnIdAndRaiseEvent()
    {
        // Arrange
        var originalColumnId = 1L;
        var newColumnId = 2L;
        var performingUserId = 123L;

        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, DateTime.Now, 1, originalColumnId, null, 1L);
        DomainTestUtils.InitializeDomainEvents(task); // Reset events

        // Act
        task.MoveToColumn(newColumnId, performingUserId);

        // Assert
        Assert.Equal(newColumnId, task.ColumnId);

        var domainEvent = Assert.Single(task.DomainEvents);
        var movedEvent = Assert.IsType<TaskMovedEvent>(domainEvent);
        Assert.Equal(originalColumnId, movedEvent.OriginalColumnId);
        Assert.Equal(newColumnId, movedEvent.NewColumnId);
        Assert.Equal(performingUserId, movedEvent.PerformedByUserId);
    }
    
    // --- Testovi za AssignToUser metodu ---

    [Fact]
    public void AssignToUser_WithNewAssignee_ShouldChangeAssigneeIdAndRaiseEvent()
    {
        // Arrange
        var oldAssigneeId = 10L;
        var newAssigneeId = 20L;
        var performingUserId = 123L;

        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, DateTime.Now, 1, 1, oldAssigneeId, 1L);
        DomainTestUtils.InitializeDomainEvents(task); // Reset events

        // Act
        task.AssignToUser(newAssigneeId, performingUserId);

        // Assert
        Assert.Equal(newAssigneeId, task.AssigneeId);

        var domainEvent = Assert.Single(task.DomainEvents);
        var assignedEvent = Assert.IsType<TaskAssignedEvent>(domainEvent);
        Assert.Equal(oldAssigneeId, assignedEvent.OldAssigneeId);
        Assert.Equal(newAssigneeId, assignedEvent.NewAssigneeId);
    }
    
    [Fact]
    public void AssignToUser_WhenAssigneeIsTheSame_ShouldDoNothing()
    {
        // Arrange
        const long assigneeId = 10L;
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, DateTime.Now, 1, 1, assigneeId, 1L);
        DomainTestUtils.InitializeDomainEvents(task); 
        
        task.AssignToUser(assigneeId, 123L);

        // Assert
        Assert.Equal(assigneeId, task.AssigneeId);
        Assert.Empty(task.DomainEvents);
    }

    // --- Activity Tracking Tests ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ShouldAddActivityToCollection()
    {
        // Arrange
        const string title = "New Task";
        const long creatorUserId = 123L;

        // Act
        var task = BoardTask.Create(title, "Description", TaskPriority.Medium, null, 1, 1L, null, creatorUserId);

        // Assert
        Assert.Single(task.Activities);
        var activity = task.Activities.First();
        Assert.Equal(ActivityType.TaskCreated, activity.Type);
        Assert.Equal(creatorUserId, activity.UserId);
        Assert.Contains(title, activity.Description);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void UpdateDetails_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Old Title", "Old Desc", TaskPriority.Low, null, 1, 1L, null, 1L);
        var initialActivityCount = task.Activities.Count;
        const long modifierUserId = 456L;

        // Act
        task.UpdateDetails("New Title", "New Desc", TaskPriority.High, DateTime.UtcNow.AddDays(5), modifierUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskUpdated, lastActivity.Type);
        Assert.Equal(modifierUserId, lastActivity.UserId);
        Assert.Contains("New Title", lastActivity.Description);
        Assert.Contains("Title", lastActivity.OldValue);
        Assert.Contains("Title", lastActivity.NewValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MoveToColumn_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, 1L, null, 1L);
        var initialActivityCount = task.Activities.Count;
        const long performingUserId = 789L;
        const long newColumnId = 2L;

        // Act
        task.MoveToColumn(newColumnId, performingUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskMoved, lastActivity.Type);
        Assert.Equal(performingUserId, lastActivity.UserId);
        Assert.Contains("OldColumnId", lastActivity.OldValue);
        Assert.Contains("NewColumnId", lastActivity.NewValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AddComment_ShouldAddActivityToCollection()
    {
        // Arrange
        // Create a proper board with columns
        var board = Board.Create("Test Board", "Description", 1L, 100L);
        var column = board.Columns.First(); // Use one of the default columns
        
        // Set the Column ID and BoardId (simulating persisted state)
        const long columnId = 10L;
        const long boardId = 5L;
        const long taskId = 20L;
        DomainTestUtils.SetPrivatePropertyValue(column, nameof(Column.Id), columnId);
        DomainTestUtils.SetPrivatePropertyValue(column, nameof(Column.BoardId), boardId);
        
        // Create task with reference to the column
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, columnId, null, 1L);
        
        // Set the Task ID (simulating persisted state)
        DomainTestUtils.SetPrivatePropertyValue(task, nameof(BoardTask.Id), taskId);
        
        // Set the Column navigation property (simulating EF Core loading)
        DomainTestUtils.SetTaskColumn(task, column);
        
        var initialActivityCount = task.Activities.Count;
        const long userId = 999L;
        const string commentContent = "This is a test comment";
 
        // Act
        task.AddComment(commentContent, userId);
 
        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.CommentAdded, lastActivity.Type);
        Assert.Equal(userId, lastActivity.UserId);
        Assert.Contains("CommentId", lastActivity.NewValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignToUser_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, 1L, null, 1L);
        var initialActivityCount = task.Activities.Count;
        const long newAssigneeId = 555L;
        const long performingUserId = 666L;

        // Act
        task.AssignToUser(newAssigneeId, performingUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskAssigned, lastActivity.Type);
        Assert.Equal(performingUserId, lastActivity.UserId);
        Assert.Contains("OldAssigneeId", lastActivity.OldValue);
        Assert.Contains("NewAssigneeId", lastActivity.NewValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Archive_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, 1L, null, 1L);
        var initialActivityCount = task.Activities.Count;
        const long modifierUserId = 777L;

        // Act
        task.Archive(modifierUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskUpdated, lastActivity.Type);
        Assert.Equal(modifierUserId, lastActivity.UserId);
        Assert.Contains("archived", lastActivity.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(false, lastActivity.OldValue["IsArchived"]);
        Assert.Equal(true, lastActivity.NewValue["IsArchived"]);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Restore_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, 1L, null, 1L);
        task.Archive(1L);
        var initialActivityCount = task.Activities.Count;
        const long modifierUserId = 888L;

        // Act
        task.Restore(modifierUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskUpdated, lastActivity.Type);
        Assert.Equal(modifierUserId, lastActivity.UserId);
        Assert.Contains("restored", lastActivity.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(true, lastActivity.OldValue["IsArchived"]);
        Assert.Equal(false, lastActivity.NewValue["IsArchived"]);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetPosition_ShouldAddActivityToCollection()
    {
        // Arrange
        var task = BoardTask.Create("Test Task", null, TaskPriority.Low, null, 1, 1L, null, 1L);
        var initialActivityCount = task.Activities.Count;
        const int newPosition = 5;
        const long modifierUserId = 999L;

        // Act
        task.SetPosition(newPosition, modifierUserId);

        // Assert
        Assert.Equal(initialActivityCount + 1, task.Activities.Count);
        var lastActivity = task.Activities.Last();
        Assert.Equal(ActivityType.TaskUpdated, lastActivity.Type);
        Assert.Equal(modifierUserId, lastActivity.UserId);
        Assert.Contains("position", lastActivity.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, lastActivity.OldValue["Position"]);
        Assert.Equal(newPosition, lastActivity.NewValue["Position"]);
    }
}
