using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.ValueObjects;

/// <summary>
/// Owned value object stored inline in BoardTask.
/// Defines how and when a task repeats.
/// </summary>
public class RecurrenceRule
{
    public RecurrenceFrequency Frequency { get; private set; }

    /// <summary>Every N units — e.g. Interval=2 + Weekly = every 2 weeks.</summary>
    public int Interval { get; private set; } = 1;

    /// <summary>
    /// Comma-separated DayOfWeek values for Weekly recurrence.
    /// Null/empty = use the day of the original task's due date.
    /// </summary>
    public string? DaysOfWeek { get; private set; }

    /// <summary>Optional hard stop date. Null = repeat indefinitely.</summary>
    public DateTime? EndDate { get; private set; }

    private RecurrenceRule() { }

    public static RecurrenceRule Create(
        RecurrenceFrequency frequency,
        int interval = 1,
        IEnumerable<DayOfWeek>? daysOfWeek = null,
        DateTime? endDate = null)
    {
        if (interval < 1)
            throw new BusinessRuleValidationException("Recurrence interval must be at least 1.");

        if (endDate.HasValue && endDate.Value.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleValidationException("Recurrence end date must be today or in the future.");

        return new RecurrenceRule
        {
            Frequency = frequency,
            Interval = interval,
            DaysOfWeek = daysOfWeek is null ? null
                : string.Join(",", daysOfWeek.Distinct().Select(d => d.ToString())),
            EndDate = endDate
        };
    }

    public IReadOnlyList<DayOfWeek> GetDaysOfWeek()
    {
        if (string.IsNullOrWhiteSpace(DaysOfWeek))
            return [];

        return DaysOfWeek
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => Enum.Parse<DayOfWeek>(d.Trim()))
            .ToList();
    }
}
