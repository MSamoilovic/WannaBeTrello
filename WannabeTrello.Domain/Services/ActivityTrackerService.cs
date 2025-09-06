using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class ActivityTrackerService:IActivityTrackerService
{
    public Task AddActivityAsync(ActivityTracker activity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}