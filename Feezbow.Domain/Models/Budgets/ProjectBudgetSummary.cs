using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Models.Budgets;

public record ProjectBudgetCurrencyOverview(
    string Currency,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalUnpaid,
    decimal OverdueAmount,
    int OverdueCount);

public record ProjectBudgetCategoryBreakdown(
    string? Category,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount);

public record ProjectBudgetMemberBreakdown(
    long UserId,
    string? FirstName,
    string? LastName,
    string Currency,
    decimal Owed,
    decimal Paid,
    decimal Outstanding);

public record ProjectBudgetSummary(
    long ProjectId,
    DateTime FromDate,
    DateTime ToDate,
    IReadOnlyList<ProjectBudgetCurrencyOverview> ByCurrency,
    IReadOnlyList<ProjectBudgetCategoryBreakdown> ByCategory,
    IReadOnlyList<ProjectBudgetMemberBreakdown> ByMember,
    IReadOnlyList<Bill> UpcomingBills,
    IReadOnlyList<Bill> OverdueBills);
