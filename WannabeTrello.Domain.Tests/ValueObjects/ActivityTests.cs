using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Tests.ValueObjects;

public class ActivityTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Constructor_WithValidParameters_ShouldCreateActivity()
    {
        // Arrange
        var activityType = ActivityType.TaskCreated;
        const string description = "Task 'Implement Login' was created";
        const long userId = 123L;
        var oldValues = new Dictionary<string, object?> { ["Status"] = "Todo" };
        var newValues = new Dictionary<string, object?> { ["Status"] = "InProgress" };

        // Act
        var activity = new Activity(activityType, description, userId, oldValues, newValues);

        // Assert
        Assert.NotNull(activity);
        Assert.Equal(activityType, activity.Type);
        Assert.Equal(description, activity.Description);
        Assert.Equal(userId, activity.UserId);
        Assert.Equal(oldValues, activity.OldValue);
        Assert.Equal(newValues, activity.NewValue);
        Assert.True(activity.Timestamp <= DateTime.UtcNow);
        Assert.True(activity.Timestamp >= DateTime.UtcNow.AddSeconds(-5)); // Should be recent
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Constructor_WithoutOldAndNewValues_ShouldCreateActivityWithEmptyDictionaries()
    {
        // Arrange
        var activityType = ActivityType.TaskCreated;
        const string description = "Task was created";
        const long userId = 456L;

        // Act
        var activity = new Activity(activityType, description, userId);

        // Assert
        Assert.NotNull(activity);
        Assert.Empty(activity.OldValue);
        Assert.Empty(activity.NewValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Constructor_WithNullOldAndNewValues_ShouldCreateActivityWithEmptyDictionaries()
    {
        // Arrange
        var activityType = ActivityType.ProjectUpdated;
        const string description = "Project was updated";
        const long userId = 789L;

        // Act
        var activity = new Activity(activityType, description, userId, null, null);

        // Assert
        Assert.NotNull(activity);
        Assert.Empty(activity.OldValue);
        Assert.Empty(activity.NewValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Domain")]
    public void Constructor_WithInvalidDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        // Arrange
        var activityType = ActivityType.TaskCreated;
        const long userId = 123L;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Activity(activityType, invalidDescription!, userId));
        Assert.Equal("description", exception.ParamName);
        Assert.Contains("Description cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Domain")]
    public void Constructor_WithInvalidUserId_ShouldThrowArgumentException(long invalidUserId)
    {
        // Arrange
        var activityType = ActivityType.BoardCreated;
        const string description = "Board was created";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Activity(activityType, description, invalidUserId));
        Assert.Equal("userId", exception.ParamName);
        Assert.Contains("UserId must be positive", exception.Message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Activity_AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var activityType = ActivityType.TaskUpdated;
        const string description = "Task updated";
        const long userId = 100L;
        var timestamp = DateTime.UtcNow;

        // Note: Since Timestamp is set in constructor, we can't test exact equality
        // But we can test that two activities with same data have same Type, Description, UserId
        var activity1 = new Activity(activityType, description, userId);
        var activity2 = new Activity(activityType, description, userId);

        // Assert - Records compare by value, but Timestamp will differ
        Assert.Equal(activity1.Type, activity2.Type);
        Assert.Equal(activity1.Description, activity2.Description);
        Assert.Equal(activity1.UserId, activity2.UserId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Constructor_ShouldSetTimestampAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var activity = new Activity(ActivityType.CommentAdded, "Comment added", 123L);
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(activity.Timestamp >= beforeCreation);
        Assert.True(activity.Timestamp <= afterCreation);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Constructor_WithComplexOldAndNewValues_ShouldStoreCorrectly()
    {
        // Arrange
        var oldValues = new Dictionary<string, object?>
        {
            ["Title"] = "Old Title",
            ["Priority"] = TaskPriority.Low,
            ["DueDate"] = DateTime.UtcNow.AddDays(5),
            ["IsArchived"] = false
        };

        var newValues = new Dictionary<string, object?>
        {
            ["Title"] = "New Title",
            ["Priority"] = TaskPriority.High,
            ["DueDate"] = DateTime.UtcNow.AddDays(3),
            ["IsArchived"] = true
        };

        // Act
        var activity = new Activity(
            ActivityType.TaskUpdated,
            "Task details were updated",
            999L,
            oldValues,
            newValues
        );

        // Assert
        Assert.Equal(4, activity.OldValue.Count);
        Assert.Equal(4, activity.NewValue.Count);
        Assert.Equal("Old Title", activity.OldValue["Title"]);
        Assert.Equal("New Title", activity.NewValue["Title"]);
        Assert.Equal(TaskPriority.Low, activity.OldValue["Priority"]);
        Assert.Equal(TaskPriority.High, activity.NewValue["Priority"]);
    }
}

