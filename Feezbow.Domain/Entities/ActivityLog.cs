using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Entities;

public class ActivityLog : AuditableEntity
{
    public Activity Activity { get; private set; } = null!;
    
    public long? BoardTaskId { get; private set; }
    public BoardTask? BoardTask { get; private set; }

    public long? ProjectId { get; private set; }
    public Project? Project { get; private set; }

    public long? BoardId { get; private set; }
    public Board? Board { get; private set; }

    private ActivityLog() { } // EF Core constructor

    public static ActivityLog CreateForTask(Activity activity, long taskId)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        if (taskId <= 0)
            throw new ArgumentException("TaskId must be positive", nameof(taskId));

        return new ActivityLog
        {
            Activity = activity,
            BoardTaskId = taskId
        };
    }

    public static ActivityLog CreateForProject(Activity activity, long projectId)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        if (projectId <= 0)
            throw new ArgumentException("ProjectId must be positive", nameof(projectId));

        return new ActivityLog
        {
            Activity = activity,
            ProjectId = projectId
        };
    }

    public static ActivityLog CreateForBoard(Activity activity, long boardId)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        if (boardId <= 0)
            throw new ArgumentException("BoardId must be positive", nameof(boardId));

        return new ActivityLog
        {
            Activity = activity,
            BoardId = boardId
        };
    }
}