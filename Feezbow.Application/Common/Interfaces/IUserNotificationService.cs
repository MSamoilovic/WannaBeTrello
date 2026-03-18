namespace WannabeTrello.Application.Common.Interfaces;

public interface IUserNotificationService
{
    Task NotifyUserProfileUpdated(
        long userId, 
        IReadOnlyDictionary<string, object?> oldValues, 
        IReadOnlyDictionary<string, object?> newValues, 
        long modifyingUserId, 
        CancellationToken cancellationToken);
    
    Task NotifyUserDeactivated(
        long userId, 
        long deactivatedByUserId, 
        CancellationToken cancellationToken);
    
    Task NotifyUserReactivated(
        long userId, 
        long reactivatedByUserId, 
        CancellationToken cancellationToken);
}

