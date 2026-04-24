using MediatR;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetSummary;

public record GetProjectBudgetSummaryQuery(
    long ProjectId,
    DateTime? From = null,
    DateTime? To = null,
    int UpcomingDays = 30) : IRequest<ProjectBudgetSummaryDto>;
