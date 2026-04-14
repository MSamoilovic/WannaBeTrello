using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Tasks.GetRecurringTasks;

public class GetRecurringTasksQueryResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskType TaskType { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? NextOccurrence { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; }
    public string? DaysOfWeek { get; set; }
    public DateTime? EndDate { get; set; }
    public long? ParentTaskId { get; set; }

    public static GetRecurringTasksQueryResponse FromEntity(BoardTask task)
    {
        return new GetRecurringTasksQueryResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            TaskType = task.TaskType,
            Priority = task.Priority,
            DueDate = task.DueDate,
            NextOccurrence = task.NextOccurrence,
            Frequency = task.Recurrence!.Frequency,
            Interval = task.Recurrence.Interval,
            DaysOfWeek = task.Recurrence.DaysOfWeek,
            EndDate = task.Recurrence.EndDate,
            ParentTaskId = task.ParentTaskId
        };
    }
}
