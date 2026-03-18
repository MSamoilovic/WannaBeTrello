using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Tests.Utils;

namespace WannabeTrello.Domain.Tests.Entities;

public class ColumnTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateColumn()
    {
        // Arrange
        var name = "To Do";
        var boardId = 1L;
        var order = 1;
        var userId = 101L;

        // Act
        var column = new Column(name, boardId, order, userId);

        // Assert
        Assert.NotNull(column);
        Assert.Equal(name, column.Name);
        Assert.Equal(boardId, column.BoardId);
        Assert.Equal(order, column.Order);
        Assert.Equal(userId, column.CreatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidName_ShouldThrowBusinessRuleValidationException(string invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            new Column(invalidName, 1L, 1, 101L));
        Assert.Equal("Column name cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidOrder_ShouldThrowBusinessRuleValidationException(int invalidOrder)
    {
        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() =>
            new Column("Valid Name", 1L, invalidOrder, 101L));
        Assert.Equal("Column order must be a positive number.", exception.Message);
    }

    // --- Testovi za ChangeName metodu ---

    [Fact]
    public void ChangeName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var column = new Column("Old Name", 1L, 1, 101L);
        var newName = "New Name";
        var modifierUserId = 102L;

        // Act
        column.ChangeName(newName, modifierUserId);

        // Assert
        Assert.Equal(newName, column.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeName_WithInvalidName_ShouldThrowBusinessRuleValidationException(string invalidName)
    {
        // Arrange
        var column = new Column("Valid Name", 1L, 1, 101L);
        var modifierUserId = 102L;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.ChangeName(invalidName, modifierUserId));
        Assert.Equal("New column name cannot be empty.", exception.Message);
    }

    // --- Testovi za ChangeOrder metodu ---

    [Fact]
    public void ChangeOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var column = new Column("Test Column", 1L, 1, 101L);
        var newOrder = 3;
        var modifierUserId = 102L;

        // Act
        column.ChangeOrder(newOrder, modifierUserId);

        // Assert
        Assert.Equal(newOrder, column.Order);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ChangeOrder_WithInvalidOrder_ShouldThrowBusinessRuleValidationException(int invalidOrder)
    {
        // Arrange
        var column = new Column("Test Column", 1L, 1, 101L);
        var modifierUserId = 102L;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.ChangeOrder(invalidOrder, modifierUserId));
        Assert.Equal("New column order must be a positive number.", exception.Message);
    }

    // --- Testovi za SetWipLimit metodu ---
    
    [Fact]
    public void SetWipLimit_WithValidLimit_ShouldUpdateWipLimit()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;

        // Act
        column.SetWipLimit(5, modifierUserId);

        // Assert
        Assert.Equal(5, column.WipLimit);
    }
    
    [Fact]
    public void SetWipLimit_WithNull_ShouldRemoveLimit()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;
        column.SetWipLimit(5, modifierUserId); // Postavi početni limit

        // Act
        column.SetWipLimit(null, modifierUserId);

        // Assert
        Assert.Null(column.WipLimit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void SetWipLimit_WithInvalidLimit_ShouldThrowBusinessRuleValidationException(int invalidLimit)
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.SetWipLimit(invalidLimit, modifierUserId));
        Assert.Equal("WIP limit must be a positive number.", exception.Message);
    }
    
    // --- Testovi za AddTask i WIP Limit logiku ---

    [Fact]
    public void AddTask_WhenWipLimitIsNotReached_ShouldAddTask()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;
        column.SetWipLimit(3, modifierUserId);
        var task1 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        var task2 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();

        // Act
        column.AddTask(task1);
        column.AddTask(task2);

        // Assert
        Assert.Equal(2, column.Tasks.Count);
    }
    
    [Fact]
    public void AddTask_WhenWipLimitIsReached_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;
        column.SetWipLimit(1, modifierUserId);
        var task1 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        column.AddTask(task1); // Limit je sada dostignut
        
        var task2 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.AddTask(task2));
        Assert.Contains("WIP limit for column 'In Progress' has been reached.", exception.Message);
        Assert.Single(column.Tasks); // Drugi task nije dodat
    }

    // --- Testovi za RemoveTask metodu ---

    [Fact]
    public void RemoveTask_WithExistingTask_ShouldRemoveTask()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var task = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        column.AddTask(task);

        // Act
        var removedTask = column.RemoveTask(task.Id);

        // Assert
        Assert.Equal(task, removedTask);
        Assert.Empty(column.Tasks);
    }

    [Fact]
    public void RemoveTask_WithNonExistingTask_ShouldThrowNotFoundException()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var nonExistingTaskId = 999L;

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => column.RemoveTask(nonExistingTaskId));
        Assert.Equal("Entity 'BoardTask' (999) was not found.", exception.Message);
    }

    // --- Testovi za HasTask metodu ---

    [Fact]
    public void HasTask_WithExistingTask_ShouldReturnTrue()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var task = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        column.AddTask(task);

        // Act
        var hasTask = column.HasTask(task.Id);

        // Assert
        Assert.True(hasTask);
    }

    [Fact]
    public void HasTask_WithNonExistingTask_ShouldReturnFalse()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var nonExistingTaskId = 999L;

        // Act
        var hasTask = column.HasTask(nonExistingTaskId);

        // Assert
        Assert.False(hasTask);
    }

    // --- Dodatni edge case testovi za WIP limit ---

    [Fact]
    public void AddTask_WhenNoWipLimit_ShouldAllowUnlimitedTasks()
    {
        // Arrange
        var column = new Column("Backlog", 1L, 1, 101L);
        // Ne postavljamo WIP limit

        // Act & Assert - Trebalo bi da možemo dodati neograničen broj taskova
        for (int i = 0; i < 10; i++)
        {
            var task = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
            column.AddTask(task);
        }

        Assert.Equal(10, column.Tasks.Count);
    }

    [Fact]
    public void SetWipLimit_WithZero_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        var modifierUserId = 102L;

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.SetWipLimit(0, modifierUserId));
        Assert.Equal("WIP limit must be a positive number.", exception.Message);
    }
}