using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Events.Project_Events;
using WannabeTrello.Domain.ValueObjects;

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

    private readonly List<Activity> _activities = [];
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();

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

        var activity = new Activity(
            ActivityType.ProjectCreated,
            $"Project '{name}' was created",
            ownerId
        );
        project.AddActivity(activity);

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.Name, ownerId, project.Description));

        return project;
    }

    public void Update(string? name, string? description, ProjectStatus? status, ProjectVisibility? visibility,
        bool archived, long updatedBy)
    {
        var changed = false;
        
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            oldValues.Add("Name", name);
            Name = name;
            changed = true;
            newValues.Add("Name", name);
        }

        if (description != null && description != Description)
        {
            oldValues.Add("Description", description);
            Description = description;
            changed = true;
            newValues.Add("Description", Description);
        }

        if (status.HasValue && status.Value != Status)
        {
            oldValues.Add("Status", status.Value);
            Status = status.Value;
            changed = true;
            newValues.Add("Status", Status);
        }

        if (visibility.HasValue && visibility.Value != Visibility)
        {
            oldValues.Add("Visibility", visibility.Value);
            Visibility = visibility.Value;
            changed = true;
            newValues.Add("Visibility", Visibility);
        }

        if (archived != IsArchived)
        {
            oldValues.Add("IsArchived", IsArchived);
            IsArchived = archived;
            changed = true;
            newValues.Add("IsArchived", IsArchived);
        }

        if (!changed) return;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        var activity = new Activity(
            ActivityType.ProjectUpdated,
            $"Project '{Name}' was updated",
            updatedBy,
            oldValues,
            newValues
        );

        AddActivity(activity);

        AddDomainEvent(new ProjectUpdatedEvent(Id, Name, updatedBy, oldValues, newValues));
    }

    public Board CreateBoard(string? name, string? description, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

        var board = Board.Create(name, description, Id, creatorUserId);
        Boards.Add(board);
        AddDomainEvent(new BoardCreatedEvent(board.Id, board.Name, board.Description, creatorUserId));

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

        var activity = new Activity(
            ActivityType.ProjectArchived,
            $"Project '{Name}' was archived",
            archiverUserId,
            newValue: new Dictionary<string, object?> { [nameof(IsArchived)] = true }
        );

        AddActivity(activity);

        AddDomainEvent(new ProjectArchivedEvent(Id, Name, archiverUserId));
    }

    public void AddMember(long newMemberId, ProjectRole role, long inviterUserId)
    {
        var inviter = ProjectMembers.FirstOrDefault(pm => pm.UserId == inviterUserId);
        if (inviter is null || (inviter.Role != ProjectRole.Owner && inviter.Role != ProjectRole.Admin))
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

        var activity = new Activity(
           ActivityType.ProjectMemberAdded,
            $"User {newMemberId} was added to project with role {role}",
           inviterUserId,
           newValue: new Dictionary<string, object?>
           {
               ["NewMemberId"] = newMemberId,
               ["Role"] = role
           }
        );
        
        AddActivity(activity);

        AddDomainEvent(new ProjectMemberAddedEvent(Id, Name,  newMemberId, role, inviterUserId));
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
        
        var role = memberToRemove.Role;
        
        ProjectMembers.Remove(memberToRemove);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = removerUserId;

        var activity = new Activity(
            ActivityType.ProjectMemberRemoved,
            $"User {removedMemberId} was removed from project",
            removerUserId,
            oldValue: new Dictionary<string, object?>
            {
                ["RemovedMemberId"] = removedMemberId,
                ["Role"] = role
            }
        );
        AddActivity(activity);

        AddDomainEvent(new ProjectMemberRemovedEvent(Id,removedMemberId, role, removerUserId));
    }

    public void UpdateMember(long updatedMemberId, ProjectRole role, long inviterUserId)
    {
        var inviter = ProjectMembers.FirstOrDefault(pm => pm.UserId == inviterUserId);
        if (inviter is null || (inviter.Role != ProjectRole.Owner && inviter.Role != ProjectRole.Admin))
        {
            throw new UnauthorizedAccessException("Only Admin or Owner can update this member's role.");
        }
        
        if (inviterUserId == updatedMemberId && inviter.Role == ProjectRole.Owner)
        {
            throw new InvalidOperationException("Owner cannot change their own role.");
        }
        
        var memberToUpdate = ProjectMembers.FirstOrDefault(pm => pm.UserId == updatedMemberId);
        if (memberToUpdate is null)
            return;
        
        var oldRole = memberToUpdate.Role;
        
        memberToUpdate.UpdateRole(role);
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = inviterUserId;

        var activity = new Activity(
            ActivityType.ProjectMemberRoleUpdated,
            $"User {updatedMemberId} role was updated from {oldRole} to {role}",
            inviterUserId,
            new Dictionary<string, object?> { ["OldRole"] = oldRole },
            new Dictionary<string, object?> { ["NewRole"] = role }
        );
        AddActivity(activity);

        AddDomainEvent(new ProjectMemberUpdatedEvent(Id, Name, updatedMemberId, oldRole, role, inviterUserId));
    }

    public bool IsMember(long memberId)
    {
        return ProjectMembers.Any(pm => pm.UserId == memberId);
    }

    public void AddActivity(Activity activity)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        _activities.Add(activity);
    }
}
