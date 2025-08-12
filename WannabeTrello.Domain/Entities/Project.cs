using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Domain.Entities;

public class Project : AuditableEntity
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public ProjectVisibility Visibility { get; private set; } = ProjectVisibility.Public;
    public bool IsArchived { get; private set; }
    public long OwnerId { get; private set; }
    public User? Owner { get; private set; }
    public ICollection<Board> Boards { get; private set; } = [];
    public ICollection<ProjectMember> ProjectMembers { get; private set; } = [];

    public static Project Create(string? name, string? description, long ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project Name cannot be empty", nameof(name));

        var project = new Project
        {
            Name = name,
            Description = description,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ownerId,
            IsArchived = false,
        };

        project.ProjectMembers.Add(new ProjectMember
        {
            UserId = ownerId,
            ProjectId = project.Id,
            Role = ProjectRole.Owner,
        });

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.Name, ownerId));

        return project;
    }

    public void Update(string? name, string? description, ProjectStatus? status, ProjectVisibility? visibility,
        bool archieved, long updatedBy)
    {
        var changed = false;

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            Name = name;
            changed = true;
        }

        if (description != null && description != Description)
        {
            Description = description;
            changed = true;
        }

        if (status.HasValue && status.Value != Status)
        {
            Status = status.Value;
            changed = true;
        }

        if (visibility.HasValue && visibility.Value != Visibility)
        {
            Visibility = visibility.Value;
            changed = true;
        }

        if (archieved != IsArchived)
        {
            IsArchived = archieved;
            changed = true;
        }

        if (!changed) return;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new ProjectUpdatedEvent(Id, Name, updatedBy));
    }

    public Board CreateBoard(string? name, string? description, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

        var board = Board.Create(name, description, Id, creatorUserId);
        Boards.Add(board);
        AddDomainEvent(new BoardCreatedEvent(board.Id, board.Name, creatorUserId));

        return board;
    }

    public void Archive(long archiverUserId)
    {
        var member = ProjectMembers.FirstOrDefault(pm => pm.UserId == archiverUserId);

        if (member == null || (member.Role != ProjectRole.Owner && member.Role != ProjectRole.Admin))
        {
            throw new UnauthorizedAccessException("Only Owner or Admin can archive this project");
        }
        
        if (IsArchived) return;
        if (this.Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be archived.");
        
        IsArchived = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = archiverUserId;
    
        AddDomainEvent(new ProjectArchivedEvent(Id));
    }
}