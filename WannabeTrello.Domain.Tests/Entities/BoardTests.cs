using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Tests.Utils;

namespace WannabeTrello.Domain.Tests.Entities;

public class BoardTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnBoardInstanceWithCorrectState()
    {
        // Arrange
        const string validName = "Development Board";
        const string validDescription = "Board for tracking development tasks.";
        const long validProjectId = 1L;
        const long creatorUserId = 101L;
        var expectedColumnNames = new[] { "To Do", "In Progress", "Done" };

        // Act
        var board = Board.Create(validName, validDescription, validProjectId, creatorUserId);
        
        Assert.NotNull(board);
        Assert.Equal(validName, board.Name);
        Assert.Equal(validDescription, board.Description);
        Assert.Equal(validProjectId, board.ProjectId);
        Assert.Equal(creatorUserId, board.CreatedBy);

        // 2. Provera datuma kreiranja (sa malom tolerancijom)
        Assert.True((DateTime.UtcNow - board.CreatedAt).TotalSeconds < 1);
        
        // 3. Provera inicijalnih kolona
        Assert.Equal(3, board.Columns.Count);
        Assert.Equal(expectedColumnNames, board.Columns.Select(c => c.Name));
        Assert.True(board.Columns.All(c => c.CreatedBy == creatorUserId));

        // 4. Provera domenskog događaja
        var boardCreatedEvent = board.DomainEvents.OfType<BoardCreatedEvent>().SingleOrDefault();
        
        Assert.NotNull(boardCreatedEvent);
        Assert.Equal(validName, boardCreatedEvent.BoardName);
        Assert.Equal(creatorUserId, boardCreatedEvent.CreatorUserId);
        Assert.Equal(board.Id, boardCreatedEvent.BoardId);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        Action act = () => Board.Create(invalidName, "Some description", 1L, 101L);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("name", exception.ParamName);
        Assert.Equal("Board Name Cannot be empty (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void Create_WithNullProjectId_ShouldThrowArgumentException()
    {
        // Arrange
        Action act = () => Board.Create("Valid Name", "Some description", null, 101L);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("projectId", exception.ParamName);
        Assert.Equal("Board must be part of the project (Parameter 'projectId')", exception.Message);
    }
    
    [Fact]
    public void UpdateDetails_WhenOnlyNameChanges_ShouldUpdateNameAndRaiseEventWithCorrectValues()
    {
        // Arrange
        var initialName = "Original Board Name";
        var initialDescription = "Original Description";
        var newName = "Updated Board Name";
        var modifierUserId = 123L;

        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.SetPrivatePropertyValue(board, "_domainEvents", new List<DomainEvent>());
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Name), initialName);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Description), initialDescription);

        // Act
        board.UpdateDetails(newName, initialDescription, modifierUserId);

        // Assert
        Assert.Equal(newName, board.Name);
        Assert.Equal(initialDescription, board.Description);

        var domainEvent = Assert.Single(board.DomainEvents);
        var boardUpdatedEvent = Assert.IsType<BoardUpdatedEvent>(domainEvent);
        
        Assert.True(boardUpdatedEvent.OldValue.ContainsKey("Name"));
        Assert.Equal(initialName, boardUpdatedEvent.OldValue["Name"]);
        
        Assert.True(boardUpdatedEvent.NewValue.ContainsKey("Name"));
        Assert.Equal(newName, boardUpdatedEvent.NewValue["Name"]);
        
        Assert.False(boardUpdatedEvent.OldValue.ContainsKey("Description"));
    }
    
    [Fact]
    public void UpdateDetails_WhenOnlyDescriptionChanges_ShouldUpdateDescriptionAndRaiseEvent()
    {
        // Arrange
        var initialName = "Original Board Name";
        var initialDescription = "Original Description";
        var newDescription = "Updated Description";
        var modifierUserId = 123L;

        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.SetPrivatePropertyValue(board, "_domainEvents", new List<DomainEvent>());
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Name), initialName);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Description), initialDescription);

        // Act
        board.UpdateDetails(initialName, newDescription, modifierUserId);

        // Assert
        Assert.Equal(initialName, board.Name);
        Assert.Equal(newDescription, board.Description);
        
        var domainEvent = Assert.Single(board.DomainEvents);
        var boardUpdatedEvent = Assert.IsType<BoardUpdatedEvent>(domainEvent);
        
        Assert.False(boardUpdatedEvent.OldValue.ContainsKey("Name"));
        Assert.True(boardUpdatedEvent.OldValue.ContainsKey("Description"));
        Assert.Equal(initialDescription, boardUpdatedEvent.OldValue["Description"]);
    }
    
    [Fact]
    public void UpdateDetails_WhenNothingChanges_ShouldNotRaiseAnyEvent()
    {
        // Arrange
        var initialName = "Original Board Name";
        var initialDescription = "Original Description";
        var modifierUserId = 123L;

        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.SetPrivatePropertyValue(board, "_domainEvents", new List<DomainEvent>());
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Name), initialName);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Description), initialDescription);

        // Act
        board.UpdateDetails(initialName, initialDescription, modifierUserId);

        // Assert
        Assert.Empty(board.DomainEvents);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.SetPrivatePropertyValue(board, "_domainEvents", new List<DomainEvent>());
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Name), "Initial Name");

        // Act
        var exception = Assert.Throws<ArgumentException>(() => 
            board.UpdateDetails(invalidName, "Some Description", 123L));

        // Assert
        Assert.Equal("newName", exception.ParamName);
        Assert.Empty(board.DomainEvents);
    }
    
    [Fact]
    public void UpdateDetails_WhenBothValuesChange_ShouldRaiseEventWithBothValues()
    {
        // Arrange
        var initialName = "Original Name";
        var initialDescription = "Original Desc";
        var newName = "New Name";
        var newDescription = "New Desc";
        var modifierUserId = 123L;

        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.SetPrivatePropertyValue(board, "_domainEvents", new List<DomainEvent>());
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Name), initialName);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.Description), initialDescription);

        // Act
        board.UpdateDetails(newName, newDescription, modifierUserId);

        // Assert
        var domainEvent = Assert.Single(board.DomainEvents);
        var boardUpdatedEvent = Assert.IsType<BoardUpdatedEvent>(domainEvent);
        
        Assert.True(boardUpdatedEvent.OldValue.ContainsKey("Name"));
        Assert.True(boardUpdatedEvent.OldValue.ContainsKey("Description"));
        Assert.Equal(initialName, boardUpdatedEvent.OldValue["Name"]);
        Assert.Equal(initialDescription, boardUpdatedEvent.OldValue["Description"]);
    }
}