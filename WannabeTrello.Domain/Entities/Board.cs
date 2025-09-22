using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class Board: AuditableEntity
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public long ProjectId { get; private set; } 
    public Project? Project { get; private set; } 
    public ICollection<Column> Columns { get; private set; } = [];
    public ICollection<BoardMember> BoardMembers { get; private set; } = [];
    public bool IsArchived { get; private set; }
    
    private Board() { }

    private Board(string name, string? description, long projectId)
    {
        Name = name;
        Description = description;
        ProjectId = projectId;
    }
    
    public static Board Create(string name, string? description, long? projectId, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board Name Cannot be empty", nameof(name));
        if (projectId is null)
            throw new ArgumentException("Board must be part of the project", nameof(projectId));

        var board = new Board(name, description, projectId.Value)
        {
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };

        board.AddColumn("To Do", 1, creatorUserId);
        board.AddColumn("In Progress", 2, creatorUserId);
        board.AddColumn("Done", 3, creatorUserId);
        
        board.AddDomainEvent(new BoardCreatedEvent(board.Id, board.Name, board.Description, creatorUserId));

        return board;
    }
    
    public void UpdateDetails(string? newName, string? newDescription, long modifierUserId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Board Name cannot be empty.", nameof(newName));

        var changed = false;
        
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        if (Name != newName)
        {
            oldValues.Add("Name", newName);
            Name = newName; 
            changed = true;
            newValues.Add("Name", newName);
        }

        if (Description != newDescription)
        {
            oldValues.Add("Description", newDescription);
            Description = newDescription; 
            changed = true;
            newValues.Add("Description", newDescription);
        }

        if (!changed) return;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        AddDomainEvent(new BoardUpdatedEvent(Id, oldValues, newValues, modifierUserId)); 
    }

    public void Archive(long modifierUserId)
    {
        var member = BoardMembers.FirstOrDefault(pm => pm.UserId == modifierUserId);
        if (member is not { Role: BoardRole.Admin })
            throw new UnauthorizedAccessException("Only Owner or Admin can archive the board.");

        if (IsArchived) return;
        
        IsArchived = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        AddDomainEvent(new BoardArchivedEvent(Id, modifierUserId));
    }

    public void Restore(long modifierUserId)
    {
        var member = BoardMembers.FirstOrDefault(pm => pm.UserId == modifierUserId);
        if (member is not { Role: BoardRole.Admin })
            throw new UnauthorizedAccessException("Only Owner or Admin can archive the board.");
        
        if(IsArchived) return;
        IsArchived = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        
        //Add DomainEvent
        AddDomainEvent(new BoardRestoredEvent(Id, modifierUserId));
    }
    
    public Column AddColumn(string columnName, int order, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Naziv kolone ne može biti prazan.", nameof(columnName));
        
        if (Columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationDomainException($"Kolona sa imenom '{columnName}' već postoji na ovoj tabli.");

        var newColumn = new Column
        {
            Name = columnName,
            Order = order,
            BoardId = Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };
        Columns.Add(newColumn);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = creatorUserId;
        
        AddDomainEvent(new ColumnAddedEvent(Id, newColumn.Id, newColumn.Name, creatorUserId));
        return newColumn;
    }
    
    public void AddMember(User user, BoardRole role, long inviterUserId)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Korisnik ne može biti null.");
        
        if (BoardMembers.Any(bm => bm.UserId == user.Id))
            throw new InvalidOperationDomainException($"Korisnik {user.FirstName} {user.LastName} je već član ove table.");

        BoardMembers.Add(new BoardMember
        {
            BoardId = Id,
            UserId = user.Id,
            Role = role
        });
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = inviterUserId;
        
    }
    
    public void RemoveMember(long userId, long removerUserId)
    {
        var memberToRemove = BoardMembers.FirstOrDefault(bm => bm.UserId == userId);
        
        if (memberToRemove == null)
            throw new NotFoundException("Član Borda", userId);

        BoardMembers.Remove(memberToRemove);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = removerUserId;

        AddDomainEvent(new BoardMemberRemovedEvent(Id, userId, removerUserId));
    }
    
    public bool IsMember(long userId)
    {
        return BoardMembers.Any(bm => bm.UserId == userId);
    }
    
    
}