using Feezbow.Domain.Enums;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Services;

/// <summary>
/// Pure domain service — calculates the next occurrence date for a recurring task.
/// Stateless; no dependencies.
/// </summary>
public static class RecurringTaskScheduler
{
    /// <summary>
    /// Calculates the next occurrence after <paramref name="from"/>.
    /// Returns null if the recurrence has expired (EndDate passed).
    /// </summary>
    public static DateTime? CalculateNext(DateTime from, RecurrenceRule rule)
    {
        if (rule.EndDate.HasValue && from.Date >= rule.EndDate.Value.Date)
            return null;

        var next = rule.Frequency switch
        {
            RecurrenceFrequency.Daily     => from.AddDays(rule.Interval),
            RecurrenceFrequency.Weekly    => CalculateNextWeekly(from, rule),
            RecurrenceFrequency.BiWeekly  => from.AddDays(14 * rule.Interval),
            RecurrenceFrequency.Monthly   => from.AddMonths(rule.Interval),
            RecurrenceFrequency.Quarterly => from.AddMonths(3 * rule.Interval),
            RecurrenceFrequency.Yearly    => from.AddYears(rule.Interval),
            _                             => (DateTime?)null
        };

        if (next is null) return null;

        // Respect EndDate
        if (rule.EndDate.HasValue && next.Value.Date > rule.EndDate.Value.Date)
            return null;

        return next.Value.Date == from.Date ? null : next;
    }

    private static DateTime? CalculateNextWeekly(DateTime from, RecurrenceRule rule)
    {
        var days = rule.GetDaysOfWeek();

        if (days.Count == 0)
            return from.AddDays(7 * rule.Interval);

        // Find the next matching day within the current or next cycle
        for (var offset = 1; offset <= 7 * rule.Interval; offset++)
        {
            var candidate = from.AddDays(offset);
            if (days.Contains(candidate.DayOfWeek))
                return candidate;
        }

        return from.AddDays(7 * rule.Interval);
    }
}
