using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events;
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
        
        if(ownerId <= 0) 
            throw new ArgumentException("Project OwnerId cannot be zero or negative", nameof(ownerId));

        var project = new Project
        {
            Name = name,
            Description = description,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ownerId,
            IsArchived = false,
        };
        
        var newProjectMember = ProjectMember.Create(ownerId, project.Id, ProjectRole.Owner);
        project.ProjectMembers.Add(newProjectMember);

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.Name, ownerId));

        return project;
    }

    public void Update(string? name, string? description, ProjectStatus? status, ProjectVisibility? visibility,
        bool archived, long updatedBy)
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

        if (archived != IsArchived)
        {
            IsArchived = archived;
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
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Only active projects can be archived.");
        
        IsArchived = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = archiverUserId;
    
        AddDomainEvent(new ProjectArchivedEvent(Id, archiverUserId));
    }

    public void AddMember(long newMemberId, ProjectRole role, long inviterUserId)
    {
        var inviter = ProjectMembers.FirstOrDefault(pm => pm.UserId == inviterUserId);
        if (inviter is null || (inviter.Role != role && inviter.Role != ProjectRole.Admin))
        {
            throw new UnauthorizedAccessException("Only Owner or Admin can add this member");
        }
        
        if(ProjectMembers.Any(pm => pm.UserId == newMemberId))
        {
            throw new  InvalidOperationException("User already exists in the project");
        }
        
        var newMember = ProjectMember.Create(newMemberId,Id, role);
        ProjectMembers.Add(newMember);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = inviterUserId;

        AddDomainEvent(new ProjectMemberAddedEvent(Id,  newMemberId, role, inviterUserId));
    }

    public void RemoveMember(long removedMemberId, long removerUserId)
    {
        var inviter = ProjectMembers.FirstOrDefault(pm => pm.UserId == removerUserId);
        if (inviter is null || (inviter.Role != ProjectRole.Owner && inviter.Role != ProjectRole.Admin))
        {
            throw new UnauthorizedAccessException("Only Admin or Owner can remove this member.");
        }

        var memberToRemove = ProjectMembers.FirstOrDefault(pm => pm.UserId == removedMemberId);
        if (memberToRemove is null)
            return;
        
        ProjectMembers.Remove(memberToRemove);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = removerUserId;
        
        AddDomainEvent(new ProjectMemberRemovedEvent(Id,removedMemberId, removerUserId));
    }

    public void UpdateMember(long updatedMemberId, ProjectRole role, long inviterUserId)
    {
        var inviter = ProjectMembers.FirstOrDefault(pm => pm.UserId == inviterUserId);
        if (inviter is null || (inviter.Role != ProjectRole.Owner && inviter.Role != ProjectRole.Admin))
        {
            throw new UnauthorizedAccessException("Only Admin or Owner can update this member's role.");
        }
        
        var memberToUpdate = ProjectMembers.FirstOrDefault(pm => pm.UserId == updatedMemberId);
        if (memberToUpdate is null)
            return;
        
        memberToUpdate.UpdateRole(role);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = inviterUserId;
        
        AddDomainEvent(new ProjectMemberUpdatedEvent(Id,  updatedMemberId, role, inviterUserId));
    }
}
