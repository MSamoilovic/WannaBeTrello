using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class UserNotificationService(
    IHubContext<TrellyHub, ITrellyHub> hubContext,
    IActivityTrackerService activityTrackerService) : IUserNotificationService
{
    public async Task NotifyUserProfileUpdated(
        long userId,
        IReadOnlyDictionary<string, object?> oldValues,
        IReadOnlyDictionary<string, object?> newValues,
        long modifyingUserId,
        CancellationToken cancellationToken)
    {
        // SignalR notification
        await hubContext.Clients.All.UserProfileUpdated(userId, modifyingUserId);

        // Build description
        var changedFields = new List<string>();
        if (oldValues.ContainsKey("FirstName"))
            changedFields.Add("first name");
        if (oldValues.ContainsKey("LastName"))
            changedFields.Add("last name");
        if (oldValues.ContainsKey("Bio"))
            changedFields.Add("bio");
        if (oldValues.ContainsKey("ProfilePictureUrl"))
            changedFields.Add("profile picture");

        var fieldsDescription = changedFields.Count > 0
            ? string.Join(", ", changedFields)
            : "profile information";

        var description = $"User profile updated: {fieldsDescription}";

        // Activity tracking
        var activity = ActivityTracker.Create(
            type: ActivityType.UserProfileUpdated,
            description: description,
            userId: modifyingUserId,
            relatedEntityId: userId,
            relatedEntityType: nameof(User),
            oldValue: oldValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            newValue: newValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }

    public async Task NotifyUserDeactivated(
        long userId,
        long deactivatedByUserId,
        CancellationToken cancellationToken)
    {
        // SignalR notification
        await hubContext.Clients.All.UserDeactivated(userId, deactivatedByUserId);

        // Build description
        var isSelfDeactivation = userId == deactivatedByUserId;
        var description = isSelfDeactivation
            ? "User deactivated their own account"
            : $"User account was deactivated by user ID {deactivatedByUserId}";

        // Activity tracking
        var newValue = new Dictionary<string, object?>
        {
            { "IsActive", false },
            { "DeactivatedAt", DateTime.UtcNow },
            { "DeactivatedBy", deactivatedByUserId }
        };

        var activity = ActivityTracker.Create(
            type: ActivityType.UserDeactivated,
            description: description,
            userId: deactivatedByUserId,
            relatedEntityId: userId,
            relatedEntityType: nameof(User),
            newValue: newValue
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }

    public async Task NotifyUserReactivated(
        long userId,
        long reactivatedByUserId,
        CancellationToken cancellationToken)
    {
        // SignalR notification
        await hubContext.Clients.All.UserReactivated(userId, reactivatedByUserId);

        // Build description
        var isSelfReactivation = userId == reactivatedByUserId;
        var description = isSelfReactivation
            ? "User reactivated their own account"
            : $"User account was reactivated by user ID {reactivatedByUserId}";

        // Activity tracking
        var newValue = new Dictionary<string, object?>
        {
            { "IsActive", true },
            { "ReactivatedAt", DateTime.UtcNow },
            { "ReactivatedBy", reactivatedByUserId },
            { "DeactivatedAt", null }
        };

        var oldValue = new Dictionary<string, object?>
        {
            { "IsActive", false }
        };

        var activity = ActivityTracker.Create(
            type: ActivityType.UserReactivated,
            description: description,
            userId: reactivatedByUserId,
            relatedEntityId: userId,
            relatedEntityType: nameof(User),
            oldValue: oldValue,
            newValue: newValue
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}

