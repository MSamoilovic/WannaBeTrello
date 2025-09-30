using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Exceptions;
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
        const string initialName = "Original Name";
        const string initialDescription = "Original Desc";
        const string newName = "New Name";
        const string newDescription = "New Desc";
        const long modifierUserId = 123L;

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
    
    [Fact]
    public void Archive_WhenCalledByAdmin_ShouldArchiveBoardAndRaiseEvent()
    {
        // Arrange
        const long adminUserId = 1L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var adminMember = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.UserId), adminUserId);
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.Role), BoardRole.Admin);
        
        var members = new List<BoardMember> { adminMember };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), false);

        // Act
        board.Archive(adminUserId);

        // Assert
        Assert.True(board.IsArchived);
        
        var domainEvent = Assert.Single(board.DomainEvents);
        var archivedEvent = Assert.IsType<BoardArchivedEvent>(domainEvent);
        Assert.Equal(adminUserId, archivedEvent.ModifierUserId);
    }

    [Fact]
    public void Archive_WhenCalledByNonAdminMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var memberUserId = 2L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var member = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(member, nameof(BoardMember.UserId), memberUserId);
        DomainTestUtils.SetPrivatePropertyValue(member, nameof(BoardMember.Role), BoardRole.Editor); 
        
        var members = new List<BoardMember> { member };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), false);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => board.Archive(memberUserId));
        Assert.Equal("Only Owner or Admin can archive the board.", exception.Message);
        Assert.False(board.IsArchived);
        Assert.Empty(board.DomainEvents);
    }

    [Fact]
    public void Archive_WhenBoardIsAlreadyArchived_ShouldDoNothing()
    {
        // Arrange
        var adminUserId = 1L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var adminMember = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.UserId), adminUserId);
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.Role), BoardRole.Admin);
        
        var members = new List<BoardMember> { adminMember };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), true); 

        // Act
        board.Archive(adminUserId);

        // Assert
        Assert.True(board.IsArchived); 
        Assert.Empty(board.DomainEvents); 
    }
    
    [Fact]
    public void Archive_WhenCalledByNonMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var nonMemberUserId = 999L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        // Tabla nema članova ili dati korisnik nije među njima
        var members = new List<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), false);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => board.Archive(nonMemberUserId));
        Assert.Equal("Only Owner or Admin can archive the board.", exception.Message);
    }
    
    [Fact]
    public void Restore_WhenCalledByAdminOnArchivedBoard_ShouldRestoreBoardAndRaiseEvent()
    {
        // Arrange
        const long adminUserId = 1L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var adminMember = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.UserId), adminUserId);
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.Role), BoardRole.Admin);
        
        var members = new List<BoardMember> { adminMember };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), true); 

        // Act
        board.Restore(adminUserId);

        // Assert
        Assert.False(board.IsArchived); 
        
        var domainEvent = Assert.Single(board.DomainEvents);
        var restoredEvent = Assert.IsType<BoardRestoredEvent>(domainEvent);
        Assert.Equal(adminUserId, restoredEvent.ModifierUserId);
    }

    [Fact]
    public void Restore_WhenCalledByNonAdminMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var memberUserId = 2L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var member = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(member, nameof(BoardMember.UserId), memberUserId);
        DomainTestUtils.SetPrivatePropertyValue(member, nameof(BoardMember.Role), BoardRole.Editor);
        
        var members = new List<BoardMember> { member };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), true);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => board.Restore(memberUserId));
        Assert.Equal("Only Owner or Admin can restore the board.", exception.Message);
        Assert.True(board.IsArchived);
        Assert.Empty(board.DomainEvents);
    }

    [Fact]
    public void Restore_WhenBoardIsNotArchived_ShouldDoNothing()
    {
        // Arrange
        var adminUserId = 1L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var adminMember = DomainTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.UserId), adminUserId);
        DomainTestUtils.SetPrivatePropertyValue(adminMember, nameof(BoardMember.Role), BoardRole.Admin);
        
        var members = new List<BoardMember> { adminMember };
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), false); // Tabla NIJE arhivirana

        // Act
        board.Restore(adminUserId);

        // Assert
        Assert.False(board.IsArchived);
        
    }

    [Fact]
    public void Restore_WhenCalledByNonMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var nonMemberUserId = 999L;
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        var members = new List<BoardMember>();
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), members);
        DomainTestUtils.SetPrivatePropertyValue(board, nameof(Board.IsArchived), true);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => board.Restore(nonMemberUserId));
        Assert.Equal("Only Owner or Admin can restore the board.", exception.Message);
    }
    

    [Fact]
    public void AddColumn_WithValidName_ShouldAddColumnWithCorrectOrderAndRaiseEvent()
    {
        // Arrange
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        
        // Postavljamo postojeću kolonu da bismo testirali logiku za redosled
        var existingColumn = DomainTestUtils.CreateInstanceWithoutConstructor<Column>();
        DomainTestUtils.SetPrivatePropertyValue(existingColumn, nameof(Column.Name), "Existing Column"); // ISPRAVKA: Dodato ime
        DomainTestUtils.SetPrivatePropertyValue(existingColumn, nameof(Column.Order), 1);
        var initialColumns = new List<Column> { existingColumn };
        DomainTestUtils.SetPrivatePropertyValue(board, "_columns", initialColumns);
        
        var newColumnName = "In Review";
        var creatorUserId = 123L;

        // Act
        board.AddColumn(newColumnName, creatorUserId);

        // Assert
        Assert.Equal(2, board.Columns.Count);
        var addedColumn = board.Columns.Last();
        Assert.Equal(newColumnName, addedColumn.Name);
        Assert.Equal(2, addedColumn.Order); // Redosled treba da bude 1 (postojeća) + 1 = 2
        
        var domainEvent = Assert.Single(board.DomainEvents);
        var columnAddedEvent = Assert.IsType<ColumnAddedEvent>(domainEvent);
        Assert.Equal(newColumnName, columnAddedEvent.ColumnName);
        Assert.Equal(creatorUserId, columnAddedEvent.CreatorUserId);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddColumn_WithInvalidName_ShouldThrowBusinessRuleValidationException(string invalidName)
    {
        // Arrange
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);
        DomainTestUtils.SetPrivatePropertyValue(board, "_columns", new List<Column>());

        // Act & Assert
        var exception = Assert.Throws<BusinessRuleValidationException>(() => board.AddColumn(invalidName, 123L));
        Assert.Equal("Column name cannot be empty.", exception.Message);
        Assert.Empty(board.Columns);
        Assert.Empty(board.DomainEvents);
    }
    
    [Fact]
    public void AddColumn_WhenColumnNameAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var board = DomainTestUtils.CreateInstanceWithoutConstructor<Board>();
        DomainTestUtils.InitializeDomainEvents(board);

        var existingColumnName = "To Do";
        var existingColumn = DomainTestUtils.CreateInstanceWithoutConstructor<Column>();
        DomainTestUtils.SetPrivatePropertyValue(existingColumn, nameof(Column.Name), existingColumnName);
        var initialColumns = new List<Column> { existingColumn };
        DomainTestUtils.SetPrivatePropertyValue(board, "_columns", initialColumns);

       
        var exception = Assert.Throws<BusinessRuleValidationException>(() => board.AddColumn("to do", 123L));
        Assert.Equal($"A column with the name 'to do' already exists on this board.", exception.Message);
        Assert.Single(board.Columns);
        Assert.Empty(board.DomainEvents);
    }
}