using Feezbow.Domain.Enums;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Tests.Services;

public class RecurringTaskSchedulerTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_Daily_ReturnsNextDay()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddDays(1), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_DailyInterval3_ReturnsThreeDaysLater()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily, 3);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddDays(3), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_WeeklyNoDays_ReturnsOneWeekLater()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Weekly);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddDays(7), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_WeeklyWithDays_PicksNextMatchingDay()
    {
        // 2026-04-14 is a Tuesday
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(
            RecurrenceFrequency.Weekly,
            1,
            new[] { DayOfWeek.Friday });

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.NotNull(next);
        Assert.Equal(DayOfWeek.Friday, next!.Value.DayOfWeek);
        Assert.Equal(new DateTime(2026, 4, 17), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_BiWeekly_AddsFourteenDays()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.BiWeekly);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddDays(14), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_Monthly_AddsOneMonth()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddMonths(1), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_Quarterly_AddsThreeMonths()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Quarterly);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddMonths(3), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_Yearly_AddsOneYear()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Yearly);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Equal(from.AddYears(1), next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_EndDatePassed_ReturnsNull()
    {
        var from = DateTime.UtcNow.Date.AddYears(1);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily, 1, endDate: from);

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Null(next);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void CalculateNext_NextExceedsEndDate_ReturnsNull()
    {
        var from = new DateTime(2026, 4, 14);
        var rule = RecurrenceRule.Create(
            RecurrenceFrequency.Weekly,
            1,
            endDate: from.AddDays(3));

        var next = RecurringTaskScheduler.CalculateNext(from, rule);

        Assert.Null(next);
    }
}
