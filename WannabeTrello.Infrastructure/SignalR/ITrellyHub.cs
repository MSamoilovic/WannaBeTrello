namespace WannabeTrello.Infrastructure.SignalR;

public interface ITrellyHub
{
        Task BoardCreated(long boardId, string? boardName, long userId);
        Task BoardUpdated(long boardId, long modifierUserId);
        Task BoardArchived(long boardId, long modifierUserId);
        Task BoardRestored(long boardId, long modifierUserId);
        Task BoardMemberAdded(string boardId, string userId, string role);
        Task BoardMemberRemoved(string boardId, string userId);

        Task ColumnCreated(long boardId, long columnId, string? columnName, long creatorUserId);
        Task ColumnUpdated(long boardId, long columnId, string oldName, string newName, long modifierUserId); 
        Task ColumnDeleted(string boardId, string columnId);
        Task ColumnOrderChanged(long boardId, long columnId, int oldOrder, int newOrder, long modifierUserId);
        Task ColumnWipLimitChanged(long boardId, long columnId, int? oldWipLimit, int? newWipLimit, long modifierUserId);
        Task ColumnDeletedEvent(long boardId, long columnId, long userId);
        
        Task TaskCreated(long taskId, string taskTitle);
        Task TaskUpdated(string boardId, string taskId, object updatedFields); 
        Task TaskDeleted(string boardId, string taskId);
        Task TaskMoved(long boardId, long newColumnId, long? performedByUserId);
        Task TaskAssigned(string boardId, string taskId, string userId);
        
        Task CommentAdded(string taskId, string commentId, string userId, string content);
        Task CommentUpdated(long taskId, long commentId);
        Task CommentDeleted(long taskId, long commentId);
        Task CommentRestored(long taskId, long commentId);
        
        Task ProjectCreated(long projectId, string? projectName, long creatorUserId);
        Task ProjectUpdated(long projectId, long creatorUserId);
        Task ProjectArchived(long projectId, long creatorUserId);
        Task AddedProjectMember(long projectId, long projectMemberId, long creatorUserId);
        Task RemovedProjectMember(long projectId, long removedUserId, long removerUserId);
        Task UpdatedProjectMember(long modifiedProjectId, long modifiedMemberId, long modifierUserId);
        
        // User events
        Task UserProfileUpdated(long userId, long modifyingUserId);
        Task UserDeactivated(long userId, long deactivatedByUserId);
        Task UserReactivated(long userId, long reactivatedByUserId);
        
        // Dodatak za praćenje aktivnosti
        Task ActivityAdded(string boardId, string activityDescription);

}