namespace Feezbow.Domain.Models.Budgets;

public record UserBudgetCurrencyOverview(
    string Currency,
    decimal TotalOwed,
    decimal TotalPaid,
    decimal TotalOutstanding);

public record UserBudgetProjectBreakdown(
    long ProjectId,
    string ProjectName,
    string Currency,
    decimal Owed,
    decimal Paid,
    decimal Outstanding);

public record UserBudgetCategoryBreakdown(
    string? Category,
    string Currency,
    decimal Owed,
    decimal Paid);

public record UserBudgetSummary(
    long UserId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<UserBudgetCurrencyOverview> ByCurrency,
    IReadOnlyList<UserBudgetProjectBreakdown> ByProject,
    IReadOnlyList<UserBudgetCategoryBreakdown> ByCategory);
