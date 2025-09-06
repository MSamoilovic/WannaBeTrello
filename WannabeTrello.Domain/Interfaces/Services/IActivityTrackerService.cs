using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IActivityTrackerService
{
    Task AddActivityAsync(ActivityTracker activity, CancellationToken cancellationToken = default);
}