using Feezbow.Domain.Enums;

namespace Feezbow.Application.Common.Interfaces;

public interface IHouseholdNotificationService
{
    Task NotifyProfileCreated(long householdId, long projectId, long createdBy, CancellationToken cancellationToken = default);
    Task NotifyProfileUpdated(long householdId, long projectId, long modifiedBy, IReadOnlyDictionary<string, object?> changes, CancellationToken cancellationToken = default);
    Task NotifyMemberRoleAssigned(long projectId, long memberId, HouseholdRole role, HouseholdRole? previousRole, long assignedBy, CancellationToken cancellationToken = default);
    Task NotifyMemberRoleRemoved(long projectId, long memberId, HouseholdRole previousRole, long removedBy, CancellationToken cancellationToken = default);
}
