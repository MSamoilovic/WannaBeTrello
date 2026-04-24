using MediatR;

namespace Feezbow.Application.Features.Budgets.GetProjectBudgetTimeline;

public record GetProjectBudgetTimelineQuery(
    long ProjectId,
    DateTime? From = null,
    DateTime? To = null,
    int Months = 12) : IRequest<ProjectBudgetTimelineDto>;
