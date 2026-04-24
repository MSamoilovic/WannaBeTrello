using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Budgets;

namespace Feezbow.Domain.Services;

public class BudgetService(
    IBillRepository billRepository,
    IProjectRepository projectRepository) : IBudgetService
{
    public async Task<ProjectBudgetSummary> GetProjectBudgetSummaryAsync(
        long projectId,
        long userId,
        DateTime from,
        DateTime to,
        int upcomingDays,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new BusinessRuleValidationException("From date cannot be after To date.");

        if (upcomingDays < 0)
            throw new BusinessRuleValidationException("UpcomingDays cannot be negative.");

        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var bills = await billRepository.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);

        var now = DateTime.UtcNow.Date;
        var upcomingCutoff = now.AddDays(upcomingDays);

        var byCurrency = bills
            .GroupBy(b => b.Currency)
            .Select(g =>
            {
                var total = g.Sum(b => b.Amount);
                var paid = g.Where(b => b.IsPaid).Sum(b => b.Amount);
                var overdue = g.Where(b => !b.IsPaid && b.DueDate.Date < now).ToList();
                return new ProjectBudgetCurrencyOverview(
                    g.Key,
                    total,
                    paid,
                    total - paid,
                    overdue.Sum(b => b.Amount),
                    overdue.Count);
            })
            .OrderBy(o => o.Currency)
            .ToList();

        var byCategory = bills
            .GroupBy(b => new { b.Category, b.Currency })
            .Select(g => new ProjectBudgetCategoryBreakdown(
                g.Key.Category,
                g.Key.Currency,
                g.Sum(b => b.Amount),
                g.Where(b => b.IsPaid).Sum(b => b.Amount)))
            .OrderBy(c => c.Currency)
            .ThenBy(c => c.Category)
            .ToList();

        var byMember = bills
            .SelectMany(b => b.Splits.Select(s => new { Bill = b, Split = s }))
            .GroupBy(x => new { x.Split.UserId, x.Bill.Currency })
            .Select(g =>
            {
                var first = g.First();
                var owed = g.Sum(x => x.Split.Amount);
                var paid = g.Where(x => x.Split.IsPaid).Sum(x => x.Split.Amount);
                return new ProjectBudgetMemberBreakdown(
                    g.Key.UserId,
                    first.Split.User?.FirstName,
                    first.Split.User?.LastName,
                    g.Key.Currency,
                    owed,
                    paid,
                    owed - paid);
            })
            .OrderBy(m => m.Currency)
            .ThenBy(m => m.UserId)
            .ToList();

        var upcoming = bills
            .Where(b => !b.IsPaid && b.DueDate.Date >= now && b.DueDate.Date <= upcomingCutoff)
            .OrderBy(b => b.DueDate)
            .ToList();

        var overdueBills = bills
            .Where(b => !b.IsPaid && b.DueDate.Date < now)
            .OrderBy(b => b.DueDate)
            .ToList();

        return new ProjectBudgetSummary(
            projectId,
            from,
            to,
            byCurrency,
            byCategory,
            byMember,
            upcoming,
            overdueBills);
    }

    public async Task<UserBudgetSummary> GetUserBudgetSummaryAsync(
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new BusinessRuleValidationException("UserId must be a positive number.");

        if (from > to)
            throw new BusinessRuleValidationException("From date cannot be after To date.");

        var bills = await billRepository.GetByUserAndDateRangeAsync(userId, from, to, cancellationToken);

        var userSlices = bills
            .Select(b => new
            {
                Bill = b,
                Split = b.Splits.First(s => s.UserId == userId)
            })
            .ToList();

        var byCurrency = userSlices
            .GroupBy(x => x.Bill.Currency)
            .Select(g =>
            {
                var owed = g.Sum(x => x.Split.Amount);
                var paid = g.Where(x => x.Split.IsPaid).Sum(x => x.Split.Amount);
                return new UserBudgetCurrencyOverview(g.Key, owed, paid, owed - paid);
            })
            .OrderBy(o => o.Currency)
            .ToList();

        var byProject = userSlices
            .GroupBy(x => new { x.Bill.ProjectId, x.Bill.Project.Name, x.Bill.Currency })
            .Select(g =>
            {
                var owed = g.Sum(x => x.Split.Amount);
                var paid = g.Where(x => x.Split.IsPaid).Sum(x => x.Split.Amount);
                return new UserBudgetProjectBreakdown(
                    g.Key.ProjectId,
                    g.Key.Name ?? string.Empty,
                    g.Key.Currency,
                    owed,
                    paid,
                    owed - paid);
            })
            .OrderBy(p => p.ProjectName)
            .ToList();

        var byCategory = userSlices
            .GroupBy(x => new { x.Bill.Category, x.Bill.Currency })
            .Select(g => new UserBudgetCategoryBreakdown(
                g.Key.Category,
                g.Key.Currency,
                g.Sum(x => x.Split.Amount),
                g.Where(x => x.Split.IsPaid).Sum(x => x.Split.Amount)))
            .OrderBy(c => c.Currency)
            .ThenBy(c => c.Category)
            .ToList();

        return new UserBudgetSummary(userId, from, to, byCurrency, byProject, byCategory);
    }

    public async Task<ProjectBudgetTimeline> GetProjectBudgetTimelineAsync(
        long projectId,
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new BusinessRuleValidationException("From date cannot be after To date.");

        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var bills = await billRepository.GetByProjectAndDateRangeAsync(projectId, from, to, cancellationToken);

        var points = bills
            .GroupBy(b => new { b.DueDate.Year, b.DueDate.Month, b.Currency })
            .Select(g => new ProjectBudgetTimelinePoint(
                g.Key.Year,
                g.Key.Month,
                g.Key.Currency,
                g.Sum(b => b.Amount),
                g.Where(b => b.IsPaid).Sum(b => b.Amount)))
            .OrderBy(p => p.Year)
            .ThenBy(p => p.Month)
            .ThenBy(p => p.Currency)
            .ToList();

        return new ProjectBudgetTimeline(projectId, from, to, points);
    }
}
