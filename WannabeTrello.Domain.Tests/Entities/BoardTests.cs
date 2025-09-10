using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events.Board_Events;

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

        // Assert
        // 1. Provera osnovnih propertija
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
}