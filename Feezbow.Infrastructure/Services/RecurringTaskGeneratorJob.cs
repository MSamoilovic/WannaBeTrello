using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Services;

namespace Feezbow.Infrastructure.Services;

/// <summary>
/// Background service that runs daily at 06:00 UTC.
/// Finds all recurring tasks whose NextOccurrence is today or earlier,
/// spawns a new task occurrence, and reschedules the parent to the next date.
/// </summary>
public sealed class RecurringTaskGeneratorJob(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringTaskGeneratorJob> logger) : BackgroundService
{
    private static readonly TimeOnly RunAt = new(6, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurringTaskGeneratorJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            logger.LogDebug("RecurringTaskGeneratorJob sleeping for {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await GenerateOccurrencesAsync(stoppingToken);
        }
    }

    private async Task GenerateOccurrencesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("RecurringTaskGeneratorJob: generating occurrences for {Date:yyyy-MM-dd}", DateTime.UtcNow.Date);

        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        const long systemUserId = 0; // system-generated tasks have no human actor

        try
        {
            var dueTasks = await uow.Tasks.GetRecurringTasksDueAsync(DateTime.UtcNow, cancellationToken);

            if (dueTasks.Count == 0)
            {
                logger.LogInformation("RecurringTaskGeneratorJob: no recurring tasks due today.");
                return;
            }

            var spawned = 0;
            var expired = 0;

            foreach (var task in dueTasks)
            {
                try
                {
                    // Create the next occurrence task
                    var occurrence = task.SpawnOccurrence(systemUserId);
                    await uow.Tasks.AddAsync(occurrence, cancellationToken);

                    // Calculate and set the next scheduled date on the parent
                    var nextDate = RecurringTaskScheduler.CalculateNext(
                        task.NextOccurrence!.Value, task.Recurrence!);

                    if (nextDate is null)
                    {
                        task.ClearRecurrence(systemUserId);
                        expired++;
                    }
                    else
                    {
                        task.ScheduleNextOccurrence(nextDate.Value, systemUserId);
                        spawned++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "RecurringTaskGeneratorJob: failed to spawn occurrence for task {TaskId}", task.Id);
                }
            }

            await uow.CompleteAsync(cancellationToken);

            logger.LogInformation(
                "RecurringTaskGeneratorJob: spawned {Spawned} occurrence(s), expired {Expired} recurrence(s).",
                spawned, expired);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RecurringTaskGeneratorJob: unhandled error during generation.");
        }
    }

    private static TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(RunAt.ToTimeSpan());

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}
