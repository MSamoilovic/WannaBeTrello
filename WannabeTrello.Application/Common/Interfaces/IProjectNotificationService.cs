namespace WannabeTrello.Application.Common.Interfaces;

public interface IProjectNotificationService
{
   public Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId);
   public Task NotifyProjectUpdated(long modifiedProjectId, long modifierUserId);
}