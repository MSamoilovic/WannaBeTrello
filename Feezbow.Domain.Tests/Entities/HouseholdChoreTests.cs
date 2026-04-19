using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Chore_Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Tests.Entities;

public class HouseholdChoreTests
{
    private const long ProjectId = 10L;
    private const long UserId = 5L;

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ValidArguments_ReturnsInitializedChore()
    {
        var chore = HouseholdChore.Create(ProjectId, "Clean kitchen", UserId);

        Assert.Equal(ProjectId, chore.ProjectId);
        Assert.Equal("Clean kitchen", chore.Title);
        Assert.Null(chore.Description);
        Assert.Null(chore.AssignedToUserId);
        Assert.Null(chore.DueDate);
        Assert.False(chore.IsCompleted);
        Assert.False(chore.IsRecurring);
        Assert.Null(chore.Recurrence);
        Assert.Equal(TaskPriority.Medium, chore.Priority);
        Assert.Equal(UserId, chore.CreatedBy);
        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreCreatedEvent>(chore.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithAllArguments_SetsAllFields()
    {
        var dueDate = DateTime.UtcNow.Date.AddDays(7);
        var recurrence = RecurrenceRule.Create(RecurrenceFrequency.Weekly);

        var chore = HouseholdChore.Create(
            ProjectId, "Vacuum", UserId,
            description: "Vacuum all rooms",
            assignedToUserId: 7L,
            dueDate: dueDate,
            recurrence: recurrence,
            priority: TaskPriority.High);

        Assert.Equal("Vacuum", chore.Title);
        Assert.Equal("Vacuum all rooms", chore.Description);
        Assert.Equal(7L, chore.AssignedToUserId);
        Assert.Equal(dueDate, chore.DueDate);
        Assert.True(chore.IsRecurring);
        Assert.NotNull(chore.Recurrence);
        Assert.Equal(TaskPriority.High, chore.Priority);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveProjectId_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdChore.Create(0, "Title", UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_EmptyTitle_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdChore.Create(ProjectId, "", UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WhitespaceTitle_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdChore.Create(ProjectId, "   ", UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveCreatedBy_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdChore.Create(ProjectId, "Title", 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_PastDueDate_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdChore.Create(ProjectId, "Title", UserId,
                dueDate: DateTime.UtcNow.Date.AddDays(-1)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_ChangesTitle_RaisesEvent()
    {
        var chore = HouseholdChore.Create(ProjectId, "Old title", UserId);
        chore.ClearDomainEvents();

        chore.Update("New title", null, null, null, UserId);

        Assert.Equal("New title", chore.Title);
        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreUpdatedEvent>(chore.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_ChangesPriority_RaisesEvent()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.ClearDomainEvents();

        chore.Update(null, null, null, TaskPriority.Urgent, UserId);

        Assert.Equal(TaskPriority.Urgent, chore.Priority);
        Assert.Single(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_NoChanges_NoOp()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.ClearDomainEvents();

        chore.Update("Title", null, null, TaskPriority.Medium, UserId);

        Assert.Empty(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_NonPositiveUpdatedBy_Throws()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            chore.Update("New", null, null, null, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_PastDueDate_Throws()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            chore.Update(null, null, DateTime.UtcNow.Date.AddDays(-1), null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Assign_ValidUserId_RaisesEvent()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.ClearDomainEvents();

        chore.Assign(7L, UserId);

        Assert.Equal(7L, chore.AssignedToUserId);
        Assert.Single(chore.DomainEvents);
        var evt = Assert.IsType<ChoreAssignedEvent>(chore.DomainEvents.First());
        Assert.Equal(7L, evt.AssignedToUserId);
        Assert.Null(evt.PreviousUserId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Assign_ChangeUser_TracksOldUser()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId, assignedToUserId: 7L);
        chore.ClearDomainEvents();

        chore.Assign(8L, UserId);

        Assert.Equal(8L, chore.AssignedToUserId);
        var evt = Assert.IsType<ChoreAssignedEvent>(chore.DomainEvents.First());
        Assert.Equal(7L, evt.PreviousUserId);
        Assert.Equal(8L, evt.AssignedToUserId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Assign_Unassign_SetsNull()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId, assignedToUserId: 7L);
        chore.ClearDomainEvents();

        chore.Assign(null, UserId);

        Assert.Null(chore.AssignedToUserId);
        Assert.Single(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Assign_SameUser_NoOp()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId, assignedToUserId: 7L);
        chore.ClearDomainEvents();

        chore.Assign(7L, UserId);

        Assert.Empty(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Assign_NonPositiveAssignedBy_Throws()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            chore.Assign(7L, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Complete_NotCompleted_SetsCompletedFields()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.ClearDomainEvents();

        var next = chore.Complete(UserId);

        Assert.True(chore.IsCompleted);
        Assert.NotNull(chore.CompletedAt);
        Assert.Equal(UserId, chore.CompletedBy);
        Assert.Null(next);
        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreCompletedEvent>(chore.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Complete_AlreadyCompleted_ReturnsNull()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.Complete(UserId);
        chore.ClearDomainEvents();

        var next = chore.Complete(UserId);

        Assert.Null(next);
        Assert.Empty(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Complete_NonPositiveCompletedBy_Throws()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            chore.Complete(0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Complete_RecurringChore_CreatesNextOccurrence()
    {
        var dueDate = DateTime.UtcNow.Date.AddDays(1);
        var recurrence = RecurrenceRule.Create(RecurrenceFrequency.Weekly);
        var chore = HouseholdChore.Create(ProjectId, "Weekly clean", UserId,
            dueDate: dueDate, recurrence: recurrence, assignedToUserId: 7L);
        chore.ClearDomainEvents();

        var next = chore.Complete(UserId);

        Assert.NotNull(next);
        Assert.Equal("Weekly clean", next!.Title);
        Assert.Equal(ProjectId, next.ProjectId);
        Assert.Equal(7L, next.AssignedToUserId);
        Assert.True(next.IsRecurring);
        Assert.NotNull(next.DueDate);
        Assert.Equal(dueDate.AddDays(7), next.DueDate);
        Assert.False(next.IsCompleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Reopen_CompletedChore_ClearsCompletedFields()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.Complete(UserId);
        chore.ClearDomainEvents();

        chore.Reopen(UserId);

        Assert.False(chore.IsCompleted);
        Assert.Null(chore.CompletedAt);
        Assert.Null(chore.CompletedBy);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Reopen_NotCompleted_NoOp()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.ClearDomainEvents();

        chore.Reopen(UserId);

        Assert.Empty(chore.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Reopen_NonPositiveReopenedBy_Throws()
    {
        var chore = HouseholdChore.Create(ProjectId, "Title", UserId);
        chore.Complete(UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            chore.Reopen(0));
    }
}
