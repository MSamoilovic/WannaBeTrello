namespace WannabeTrello.Application.Common.Interfaces;

public interface IProjectNotificationService
{
   public Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId);
   public Task NotifyProjectUpdated(long modifiedProjectId, long modifierUserId);
   public Task NotifyProjectArchived(long projectId, long modifierUserId);
   public Task NotifyProjectMemberAdded(long projectId, long newMemberId, string? projectName, long inviterUserId);
   
}