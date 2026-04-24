namespace Feezbow.Application.Features.Budgets.Common;

internal static class BudgetDateRange
{
    /// <summary>
    /// Resolves (from, to) to the current month when either is null. Otherwise returns the caller's values,
    /// normalizing From to the start of the day and To to the end of the day (23:59:59.999).
    /// </summary>
    public static (DateTime From, DateTime To) ResolveMonthly(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue)
            return (from.Value.Date, to.Value.Date.AddDays(1).AddTicks(-1));

        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        return (start, end);
    }

    /// <summary>
    /// Resolves a rolling window ending today, spanning the given number of months (inclusive of current month).
    /// </summary>
    public static (DateTime From, DateTime To) ResolveRollingMonths(DateTime? from, DateTime? to, int months)
    {
        if (from.HasValue && to.HasValue)
            return (from.Value.Date, to.Value.Date.AddDays(1).AddTicks(-1));

        var now = DateTime.UtcNow;
        var end = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1);
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));
        return (start, end);
    }
}
