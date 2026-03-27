using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Feezbow.Infrastructure.Services;

public sealed class DueTaskReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<DueTaskReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DueTaskReminderService started.");

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        // Run immediately on startup, then every 24 hours
        do
        {
            await SendRemindersAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for tasks due tomorrow.");

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var dayAfterTomorrow = tomorrow.AddDays(1);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<IBoardTaskRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var tasks = await taskRepository.GetTasksDueSoonAsync(tomorrow, dayAfterTomorrow, cancellationToken);

            logger.LogInformation("Found {Count} task(s) due tomorrow.", tasks.Count);

            foreach (var task in tasks)
            {
                var assignee = task.Assignee;
                if (assignee is null || string.IsNullOrWhiteSpace(assignee.Email))
                    continue;

                if (!assignee.IsActive || !assignee.EmailConfirmed)
                    continue;

                try
                {
                    await emailService.SendTaskDueReminderEmailAsync(
                        assignee.Email,
                        assignee.DisplayName,
                        task.Title,
                        task.DueDate!.Value,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Failed to send due-date reminder for task {TaskId} to user {UserId}.",
                        task.Id, assignee.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while sending task due-date reminders.");
        }
    }
}
