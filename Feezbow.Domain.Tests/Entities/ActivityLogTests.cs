using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Tests.Entities;

public class ActivityLogTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForTask_WithValidParameters_ShouldCreateActivityLog()
    {
        // Arrange
        var activity = new Activity(
            ActivityType.TaskCreated,
            "Task 'Implement Feature' was created",
            123L
        );
        const long taskId = 456L;

        // Act
        var activityLog = ActivityLog.CreateForTask(activity, taskId);

        // Assert
        Assert.NotNull(activityLog);
        Assert.Equal(activity, activityLog.Activity);
        Assert.Equal(taskId, activityLog.BoardTaskId);
        Assert.Null(activityLog.ProjectId);
        Assert.Null(activityLog.BoardId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForProject_WithValidParameters_ShouldCreateActivityLog()
    {
        // Arrange
        var activity = new Activity(
            ActivityType.ProjectCreated,
            "Project 'New Project' was created",
            789L
        );
        const long projectId = 111L;

        // Act
        var activityLog = ActivityLog.CreateForProject(activity, projectId);

        // Assert
        Assert.NotNull(activityLog);
        Assert.Equal(activity, activityLog.Activity);
        Assert.Equal(projectId, activityLog.ProjectId);
        Assert.Null(activityLog.BoardTaskId);
        Assert.Null(activityLog.BoardId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForBoard_WithValidParameters_ShouldCreateActivityLog()
    {
        // Arrange
        var activity = new Activity(
            ActivityType.BoardCreated,
            "Board 'Sprint Board' was created",
            222L
        );
        const long boardId = 333L;

        // Act
        var activityLog = ActivityLog.CreateForBoard(activity, boardId);

        // Assert
        Assert.NotNull(activityLog);
        Assert.Equal(activity, activityLog.Activity);
        Assert.Equal(boardId, activityLog.BoardId);
        Assert.Null(activityLog.BoardTaskId);
        Assert.Null(activityLog.ProjectId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForTask_WithNullActivity_ShouldThrowArgumentNullException()
    {
        // Arrange
        const long taskId = 456L;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            ActivityLog.CreateForTask(null!, taskId));
        Assert.Equal("activity", exception.ParamName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForProject_WithNullActivity_ShouldThrowArgumentNullException()
    {
        // Arrange
        const long projectId = 789L;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            ActivityLog.CreateForProject(null!, projectId));
        Assert.Equal("activity", exception.ParamName);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForBoard_WithNullActivity_ShouldThrowArgumentNullException()
    {
        // Arrange
        const long boardId = 123L;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            ActivityLog.CreateForBoard(null!, boardId));
        Assert.Equal("activity", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    [Trait("Category", "Domain")]
    public void CreateForTask_WithInvalidTaskId_ShouldThrowArgumentException(long invalidTaskId)
    {
        // Arrange
        var activity = new Activity(
            ActivityType.TaskCreated,
            "Task was created",
            123L
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ActivityLog.CreateForTask(activity, invalidTaskId));
        Assert.Equal("taskId", exception.ParamName);
        Assert.Contains("TaskId must be positive", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    [Trait("Category", "Domain")]
    public void CreateForProject_WithInvalidProjectId_ShouldThrowArgumentException(long invalidProjectId)
    {
        // Arrange
        var activity = new Activity(
            ActivityType.ProjectCreated,
            "Project was created",
            456L
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ActivityLog.CreateForProject(activity, invalidProjectId));
        Assert.Equal("projectId", exception.ParamName);
        Assert.Contains("ProjectId must be positive", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    [Trait("Category", "Domain")]
    public void CreateForBoard_WithInvalidBoardId_ShouldThrowArgumentException(long invalidBoardId)
    {
        // Arrange
        var activity = new Activity(
            ActivityType.BoardCreated,
            "Board was created",
            789L
        );

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ActivityLog.CreateForBoard(activity, invalidBoardId));
        Assert.Equal("boardId", exception.ParamName);
        Assert.Contains("BoardId must be positive", exception.Message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CreateForTask_WithComplexActivity_ShouldPreserveActivityData()
    {
        // Arrange
        var oldValues = new Dictionary<string, object?>
        {
            ["Title"] = "Old Title",
            ["Priority"] = TaskPriority.Low
        };
        var newValues = new Dictionary<string, object?>
        {
            ["Title"] = "New Title",
            ["Priority"] = TaskPriority.High
        };

        var activity = new Activity(
            ActivityType.TaskUpdated,
            "Task was updated",
            999L,
            oldValues,
            newValues
        );
        const long taskId = 555L;

        // Act
        var activityLog = ActivityLog.CreateForTask(activity, taskId);

        // Assert
        Assert.Equal(ActivityType.TaskUpdated, activityLog.Activity.Type);
        Assert.Equal("Task was updated", activityLog.Activity.Description);
        Assert.Equal(999L, activityLog.Activity.UserId);
        Assert.Equal(2, activityLog.Activity.OldValue.Count);
        Assert.Equal(2, activityLog.Activity.NewValue.Count);
        Assert.Equal("Old Title", activityLog.Activity.OldValue["Title"]);
        Assert.Equal("New Title", activityLog.Activity.NewValue["Title"]);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void ActivityLog_ShouldInheritFromAuditableEntity()
    {
        // Arrange
        var activity = new Activity(ActivityType.TaskCreated, "Task created", 123L);
        
        // Act
        var activityLog = ActivityLog.CreateForTask(activity, 456L);

        // Assert - ActivityLog should have AuditableEntity properties
        Assert.IsAssignableFrom<AuditableEntity>(activityLog);
        // These properties exist on AuditableEntity (inherited from BaseEntity)
        Assert.Equal(0, activityLog.Id); // Default value before persisting
    }
}

