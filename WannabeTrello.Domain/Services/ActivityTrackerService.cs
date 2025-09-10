using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class ActivityTrackerService(IActivityTrackerRepository activityTrackerRepository, IUnitOfWork unitOfWork)
    : IActivityTrackerService
{
    public async Task AddActivityAsync(ActivityTracker activity, CancellationToken cancellationToken = default)
    {
        await activityTrackerRepository.AddAsync(activity, cancellationToken);
    }
}