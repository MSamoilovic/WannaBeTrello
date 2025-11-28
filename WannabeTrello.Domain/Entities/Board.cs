using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Entities;

public class Board: AuditableEntity
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public long ProjectId { get; private set; } 
    public Project? Project { get; private set; } 
    
    private readonly List<Column> _columns = [];
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
   
    public ICollection<BoardMember> BoardMembers { get; private set; } = [];
    public bool IsArchived { get; private set; }

    private readonly List<Activity> _activities = [];
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();

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

        board.AddDefaultColumns(creatorUserId);

        var activity = new Activity(
            ActivityType.BoardCreated,
            $"Board '{name}' was created",
            creatorUserId
        );
        board.AddActivity(activity);

        board.AddDomainEvent(new BoardCreatedEvent(board.Id, board.Name, board.Description, creatorUserId));

        return board;
    }
    
    private void AddDefaultColumns(long creatorUserId)
    {
        
        _columns.Add(new Column("To Do", Id, 1, creatorUserId));
        _columns.Add(new Column("In Progress", Id, 2, creatorUserId));
        _columns.Add(new Column("Done", Id, 3, creatorUserId));
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
            oldValues.Add("Name", Name);
            Name = newName; 
            changed = true;
            newValues.Add("Name", newName);
        }

        if (Description != newDescription)
        {
            oldValues.Add("Description", Description);
            Description = newDescription; 
            changed = true;
            newValues.Add("Description", newDescription);
        }

        if (!changed) return;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;

        var activity = new Activity(
            ActivityType.BoardUpdated,
            $"Board '{newName}' was updated",
            modifierUserId,
            oldValues,
            newValues
        );
        AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.BoardArchived,
            $"Board '{Name}' was archived",
            modifierUserId,
            newValue: new Dictionary<string, object?> { [nameof(IsArchived)] = true }
        );
        AddActivity(activity);

        AddDomainEvent(new BoardArchivedEvent(Id, modifierUserId));
    }

    public void Restore(long modifierUserId)
    {
        var member = BoardMembers.FirstOrDefault(pm => pm.UserId == modifierUserId);
        if (member is not { Role: BoardRole.Admin })
            throw new UnauthorizedAccessException("Only Owner or Admin can restore the board.");
        
        IsArchived = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;

        var activity = new Activity(
            ActivityType.BoardRestored,
            $"Board '{Name}' was restored",
            modifierUserId,
            new Dictionary<string, object?> { [nameof(IsArchived)] = true },
            new Dictionary<string, object?> { [nameof(IsArchived)] = false }
        );
        AddActivity(activity);

        AddDomainEvent(new BoardRestoredEvent(Id, modifierUserId));
    }
    
    public void AddColumn(string columnName, long creatorUserId)
    {
        
        if (string.IsNullOrWhiteSpace(columnName))
            throw new BusinessRuleValidationException("Column name cannot be empty.");
    
        if (_columns.Any(c => c.Name!.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleValidationException($"A column with the name '{columnName}' already exists on this board.");
        
        var order = _columns.Count != 0 ? _columns.Max(c => c.Order) + 1 : 1;
        var newColumn = new Column(columnName, Id, order, creatorUserId);
    
        _columns.Add(newColumn);

        var activity = new Activity(
            ActivityType.ColumnAdded,
            $"Column '{columnName}' was added",
            creatorUserId,
            newValue: new Dictionary<string, object?>
            {
                ["ColumnName"] = columnName,
                ["Order"] = order
            }
        );
        AddActivity(activity);

        AddDomainEvent(new ColumnAddedEvent(Id, newColumn.Id, newColumn.Name!, creatorUserId));
    
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

    public void AddActivity(Activity activity)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        _activities.Add(activity);
    }

}