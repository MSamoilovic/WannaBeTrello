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

        // Act
        column.ChangeName(newName);

        // Assert
        Assert.Equal(newName, column.Name);
    }

    // --- Testovi za SetWipLimit metodu ---
    
    [Fact]
    public void SetWipLimit_WithValidLimit_ShouldUpdateWipLimit()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);

        // Act
        column.SetWipLimit(5);

        // Assert
        Assert.Equal(5, column.WipLimit);
    }
    
    [Fact]
    public void SetWipLimit_WithNull_ShouldRemoveLimit()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        column.SetWipLimit(5); // Postavi početni limit

        // Act
        column.SetWipLimit(null);

        // Assert
        Assert.Null(column.WipLimit);
    }
    
    // --- Testovi za AddTask i WIP Limit logiku ---

    [Fact]
    public void AddTask_WhenWipLimitIsNotReached_ShouldAddTask()
    {
        // Arrange
        var column = new Column("In Progress", 1L, 2, 101L);
        column.SetWipLimit(3);
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
        column.SetWipLimit(1);
        var task1 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        column.AddTask(task1); // Limit je sada dostignut
        
        var task2 = DomainTestUtils.CreateInstanceWithoutConstructor<BoardTask>();

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => column.AddTask(task2));
        Assert.Contains("WIP limit for column 'In Progress' has been reached.", exception.Message);
        Assert.Single(column.Tasks); // Drugi task nije dodat
    }
}