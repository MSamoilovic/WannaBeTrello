using MediatR;

namespace Feezbow.Application.Features.Budgets.GetUserBudgetSummary;

public record GetUserBudgetSummaryQuery(
    DateTime? From = null,
    DateTime? To = null) : IRequest<UserBudgetSummaryDto>;
