using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Chores.GetChoresByProject;

public record ChoreDto(
    long Id,
    string Title,
    string? Description,
    long? AssignedToUserId,
    string? AssignedToName,
    DateTime? DueDate,
    bool IsCompleted,
    DateTime? CompletedAt,
    long? CompletedBy,
    bool IsRecurring,
    TaskPriority Priority,
    DateTime CreatedAt)
{
    public static ChoreDto FromEntity(HouseholdChore chore) => new(
        chore.Id,
        chore.Title,
        chore.Description,
        chore.AssignedToUserId,
        chore.AssignedTo is not null
            ? $"{chore.AssignedTo.FirstName} {chore.AssignedTo.LastName}".Trim()
            : null,
        chore.DueDate,
        chore.IsCompleted,
        chore.CompletedAt,
        chore.CompletedBy,
        chore.IsRecurring,
        chore.Priority,
        chore.CreatedAt);
}
