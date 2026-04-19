using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Chore_Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Entities;

public class HouseholdChore : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public long? AssignedToUserId { get; private set; }
    public User? AssignedTo { get; private set; }

    public DateTime? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? CompletedBy { get; private set; }

    public bool IsRecurring { get; private set; }
    public RecurrenceRule? Recurrence { get; private set; }

    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;

    private HouseholdChore() { }

    public static HouseholdChore Create(
        long projectId,
        string title,
        long createdBy,
        string? description = null,
        long? assignedToUserId = null,
        DateTime? dueDate = null,
        RecurrenceRule? recurrence = null,
        TaskPriority priority = TaskPriority.Medium)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Chore title cannot be empty.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        if (dueDate.HasValue && dueDate.Value.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleValidationException("Due date cannot be in the past.");

        var chore = new HouseholdChore
        {
            ProjectId = projectId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            IsCompleted = false,
            IsRecurring = recurrence is not null,
            Recurrence = recurrence,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        chore.AddDomainEvent(new ChoreCreatedEvent(chore.Id, projectId, chore.Title, assignedToUserId, createdBy));
        return chore;
    }

    public void Update(string? title, string? description, DateTime? dueDate, TaskPriority? priority, long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        var changed = false;

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalized = title.Trim();
            if (!string.Equals(Title, normalized, StringComparison.Ordinal))
            {
                Title = normalized;
                changed = true;
            }
        }

        if (description is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (!string.Equals(Description, normalized, StringComparison.Ordinal))
            {
                Description = normalized;
                changed = true;
            }
        }

        if (dueDate.HasValue)
        {
            if (dueDate.Value.Date < DateTime.UtcNow.Date)
                throw new BusinessRuleValidationException("Due date cannot be in the past.");

            if (DueDate != dueDate.Value)
            {
                DueDate = dueDate.Value;
                changed = true;
            }
        }

        if (priority.HasValue && Priority != priority.Value)
        {
            Priority = priority.Value;
            changed = true;
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new ChoreUpdatedEvent(Id, ProjectId, updatedBy));
    }

    public void Assign(long? userId, long assignedBy)
    {
        if (assignedBy <= 0)
            throw new BusinessRuleValidationException("AssignedBy must be a positive number.");

        if (AssignedToUserId == userId) return;

        var previousUserId = AssignedToUserId;
        AssignedToUserId = userId;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = assignedBy;

        AddDomainEvent(new ChoreAssignedEvent(Id, ProjectId, userId, previousUserId, assignedBy));
    }

    public HouseholdChore? Complete(long completedBy)
    {
        if (completedBy <= 0)
            throw new BusinessRuleValidationException("CompletedBy must be a positive number.");

        if (IsCompleted) return null;

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = completedBy;

        AddDomainEvent(new ChoreCompletedEvent(Id, ProjectId, completedBy));

        if (!IsRecurring || Recurrence is null) return null;

        var nextDue = RecurringTaskScheduler.CalculateNext(DueDate ?? DateTime.UtcNow, Recurrence);
        if (nextDue is null) return null;

        return Create(
            ProjectId,
            Title,
            completedBy,
            Description,
            AssignedToUserId,
            nextDue.Value,
            Recurrence,
            Priority);
    }

    public void Reopen(long reopenedBy)
    {
        if (reopenedBy <= 0)
            throw new BusinessRuleValidationException("ReopenedBy must be a positive number.");

        if (!IsCompleted) return;

        IsCompleted = false;
        CompletedAt = null;
        CompletedBy = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = reopenedBy;
    }
}
