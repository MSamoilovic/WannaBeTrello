namespace WannabeTrello.Application.Common.Caching;

public static class CacheKeys
{
    public static string Board(long boardId) => $"board:{boardId}";
    public static string BoardColumns(long boardId) => $"board:{boardId}:columns";
    public static string BoardTasks(long boardId) => $"board:{boardId}:tasks";
    public static string BoardActivity(long boardId) => $"activity:board:{boardId}";

    public static string Project(long projectId) => $"project:{projectId}";
    public static string ProjectMembers(long projectId) => $"project:{projectId}:members";
    public static string ProjectBoards(long projectId) => $"project:{projectId}:boards";

    public static string UserProfile(long userId) => $"user:{userId}:profile";
    public static string UserProjects(long userId) => $"user:{userId}:projects";
    public static string UserBoards(long userId) => $"user:{userId}:boards";
    public static string UserTasks(long userId) => $"user:{userId}:tasks";

    public static string Task(long taskId) => $"task:{taskId}";
    public static string TaskComments(long taskId) => $"task:{taskId}:comments";

    public static string Column(long columnId) => $"column:{columnId}";

}
