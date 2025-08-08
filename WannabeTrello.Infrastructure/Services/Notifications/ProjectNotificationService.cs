using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class ProjectNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IProjectNotificationService
{
    public async Task NotifyProjectCreated(long createdProjectId, string? projectName, long creatorUserId)
    {
        await hubContext.Clients.All.ProjectCreated(createdProjectId, projectName, creatorUserId);
    }
}