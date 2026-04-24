using Feezbow.Domain.Models.Budgets;

namespace Feezbow.Application.Features.Budgets.GetUserBudgetSummary;

public record UserBudgetCurrencyOverviewDto(
    string Currency,
    decimal TotalOwed,
    decimal TotalPaid,
    decimal TotalOutstanding);

public record UserBudgetProjectBreakdownDto(
    long ProjectId,
    string ProjectName,
    string Currency,
    decimal Owed,
    decimal Paid,
    decimal Outstanding);

public record UserBudgetCategoryBreakdownDto(
    string? Category,
    string Currency,
    decimal Owed,
    decimal Paid);

public record UserBudgetSummaryDto(
    long UserId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<UserBudgetCurrencyOverviewDto> ByCurrency,
    IReadOnlyList<UserBudgetProjectBreakdownDto> ByProject,
    IReadOnlyList<UserBudgetCategoryBreakdownDto> ByCategory)
{
    public static UserBudgetSummaryDto FromDomain(UserBudgetSummary s) => new(
        s.UserId,
        s.FromDate,
        s.ToDate,
        s.ByCurrency.Select(c => new UserBudgetCurrencyOverviewDto(
            c.Currency, c.TotalOwed, c.TotalPaid, c.TotalOutstanding)).ToList(),
        s.ByProject.Select(p => new UserBudgetProjectBreakdownDto(
            p.ProjectId, p.ProjectName, p.Currency, p.Owed, p.Paid, p.Outstanding)).ToList(),
        s.ByCategory.Select(c => new UserBudgetCategoryBreakdownDto(
            c.Category, c.Currency, c.Owed, c.Paid)).ToList());
}
