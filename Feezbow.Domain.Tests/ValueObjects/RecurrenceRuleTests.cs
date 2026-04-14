using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Tests.ValueObjects;

public class RecurrenceRuleTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Create_DefaultInterval_ReturnsRuleWithIntervalOne()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily);

        Assert.Equal(RecurrenceFrequency.Daily, rule.Frequency);
        Assert.Equal(1, rule.Interval);
        Assert.Null(rule.DaysOfWeek);
        Assert.Null(rule.EndDate);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_InvalidInterval_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            RecurrenceRule.Create(RecurrenceFrequency.Weekly, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_EndDateInPast_ThrowsBusinessRuleValidationException()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            RecurrenceRule.Create(RecurrenceFrequency.Daily, endDate: DateTime.UtcNow.Date.AddDays(-1)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithDaysOfWeek_SerializesToCsv()
    {
        var rule = RecurrenceRule.Create(
            RecurrenceFrequency.Weekly,
            1,
            new[] { DayOfWeek.Monday, DayOfWeek.Friday });

        Assert.NotNull(rule.DaysOfWeek);
        Assert.Contains("Monday", rule.DaysOfWeek);
        Assert.Contains("Friday", rule.DaysOfWeek);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithDuplicateDays_DistinctsDaysOfWeek()
    {
        var rule = RecurrenceRule.Create(
            RecurrenceFrequency.Weekly,
            1,
            new[] { DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Friday });

        var parsed = rule.GetDaysOfWeek();
        Assert.Equal(2, parsed.Count);
        Assert.Contains(DayOfWeek.Monday, parsed);
        Assert.Contains(DayOfWeek.Friday, parsed);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void GetDaysOfWeek_WhenNull_ReturnsEmpty()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Daily);
        Assert.Empty(rule.GetDaysOfWeek());
    }
}
