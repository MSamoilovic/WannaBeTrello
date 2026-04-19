using Feezbow.Domain.Enums;
using MediatR;

namespace Feezbow.Application.Features.Chores.CreateChore;

public record CreateChoreCommand(
    long ProjectId,
    string Title,
    string? Description = null,
    long? AssignedToUserId = null,
    DateTime? DueDate = null,
    TaskPriority Priority = TaskPriority.Medium,
    RecurrenceFrequency? RecurrenceFrequency = null,
    int RecurrenceInterval = 1,
    IEnumerable<DayOfWeek>? RecurrenceDaysOfWeek = null,
    DateTime? RecurrenceEndDate = null) : IRequest<CreateChoreCommandResponse>;
