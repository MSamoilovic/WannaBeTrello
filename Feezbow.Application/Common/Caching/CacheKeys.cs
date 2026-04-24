namespace Feezbow.Application.Common.Caching;

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

    public static string BoardLabels(long boardId) => $"board:{boardId}:labels";

    public static string HouseholdProfile(long projectId) => $"household:{projectId}:profile";
    public static string HouseholdMembers(long projectId) => $"household:{projectId}:members";

    public static string ProjectChores(long projectId) => $"project:{projectId}:chores";
    public static string Chore(long choreId) => $"chore:{choreId}";

    public static string ProjectShoppingLists(long projectId) => $"project:{projectId}:shopping-lists";
    public static string ShoppingList(long listId) => $"shopping-list:{listId}";

    public static string ProjectBills(long projectId) => $"project:{projectId}:bills";
    public static string Bill(long billId) => $"bill:{billId}";

    public static string ProjectBudgetSummary(long projectId, DateTime from, DateTime to, int upcomingDays) =>
        $"project:{projectId}:budget:summary:{from:yyyyMMdd}:{to:yyyyMMdd}:u{upcomingDays}";

    public static string ProjectBudgetSummaryPrefix(long projectId) =>
        $"project:{projectId}:budget:";

    public static string ProjectBudgetTimeline(long projectId, DateTime from, DateTime to) =>
        $"project:{projectId}:budget:timeline:{from:yyyyMMdd}:{to:yyyyMMdd}";

    public static string UserBudgetSummary(long userId, DateTime from, DateTime to) =>
        $"user:{userId}:budget:{from:yyyyMMdd}:{to:yyyyMMdd}";

    public static string UserBudgetPrefix(long userId) =>
        $"user:{userId}:budget:";
}
