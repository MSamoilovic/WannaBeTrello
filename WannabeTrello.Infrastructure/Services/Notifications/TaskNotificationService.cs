using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class TaskNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): ITaskNotificationService
{
    public async Task NotifyTaskCreated(long taskId, string taskTitle)
    {
        await hubContext.Clients.All.TaskCreated(taskId, taskTitle);
    }
}