using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Services;

namespace Feezbow.Infrastructure.Services;

/// <summary>
/// Background service that runs daily at 06:15 UTC.
/// Finds all recurring bill templates whose NextOccurrence is today or earlier,
/// spawns a new bill occurrence (carrying a copy of the template's splits), and
/// advances the template's NextOccurrence. If the recurrence has expired, clears it.
/// </summary>
public sealed class RecurringBillGeneratorJob(
    IServiceScopeFactory scopeFactory,
    ILogger<RecurringBillGeneratorJob> logger) : BackgroundService
{
    private static readonly TimeOnly RunAt = new(6, 15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RecurringBillGeneratorJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            logger.LogDebug("RecurringBillGeneratorJob sleeping for {Delay}", delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await GenerateOccurrencesAsync(stoppingToken);
        }
    }

    private async Task GenerateOccurrencesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("RecurringBillGeneratorJob: generating occurrences for {Date:yyyy-MM-dd}", DateTime.UtcNow.Date);

        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        const long systemUserId = 0;

        try
        {
            var dueBills = await uow.Bills.GetRecurringBillsDueAsync(DateTime.UtcNow, cancellationToken);

            if (dueBills.Count == 0)
            {
                logger.LogInformation("RecurringBillGeneratorJob: no recurring bills due today.");
                return;
            }

            var spawned = 0;
            var expired = 0;

            foreach (var template in dueBills)
            {
                try
                {
                    var occurrence = template.SpawnOccurrence(systemUserId);
                    await uow.Bills.AddAsync(occurrence, cancellationToken);

                    var nextDate = RecurringTaskScheduler.CalculateNext(
                        template.NextOccurrence!.Value, template.Recurrence!);

                    if (nextDate is null)
                    {
                        template.ClearRecurrence(systemUserId);
                        expired++;
                    }
                    else
                    {
                        template.ScheduleNextOccurrence(nextDate.Value, systemUserId);
                        spawned++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "RecurringBillGeneratorJob: failed to spawn occurrence for bill {BillId}", template.Id);
                }
            }

            await uow.CompleteAsync(cancellationToken);

            logger.LogInformation(
                "RecurringBillGeneratorJob: spawned {Spawned} occurrence(s), expired {Expired} recurrence(s).",
                spawned, expired);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RecurringBillGeneratorJob: unhandled error during generation.");
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
