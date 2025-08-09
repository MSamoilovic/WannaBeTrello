namespace WannabeTrello.Infrastructure.SignalR;

public interface ITrellyHub
{
        Task BoardCreated(long boardId, string boardName, long userId);
        Task BoardUpdated(long boardId, long modifierUserId);
        Task BoardDeleted(string boardId);
        Task BoardMemberAdded(string boardId, string userId, string role);
        Task BoardMemberRemoved(string boardId, string userId);

        Task ColumnCreated(string boardId, string columnId, string columnName);
        Task ColumnUpdated(string boardId, string columnId, string columnName); 
        Task ColumnDeleted(string boardId, string columnId);
        Task ColumnOrderChanged(string boardId, string[] columnIdsInOrder);
        Task TaskCreated(long taskId, string taskTitle);
        Task TaskUpdated(string boardId, string taskId, object updatedFields); 
        Task TaskDeleted(string boardId, string taskId);
        Task TaskMoved(string boardId, string taskId, string oldColumnId, string newColumnId);
        Task TaskAssigned(string boardId, string taskId, string userId);
        
        Task CommentAdded(string taskId, string commentId, string userId, string content);
        Task CommentUpdated(string taskId, string commentId, string content);
        Task CommentDeleted(string taskId, string commentId);
        
        Task ProjectCreated(long projectId, string? projectName, long creatorUserId);
        Task ProjectUpdated(long projectId, long creatorUserId);
        
        // Dodatak za praćenje aktivnosti
        Task ActivityAdded(string boardId, string activityDescription);

}