using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Services;

public class BillService(
    IBillRepository billRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IBillService
{
    public async Task<Bill> CreateBillAsync(
        long projectId,
        long userId,
        string title,
        decimal amount,
        DateTime dueDate,
        string currency,
        string? description,
        string? category,
        IReadOnlyCollection<long>? splitUserIds,
        RecurrenceFrequency? recurrenceFrequency,
        int recurrenceInterval,
        IEnumerable<DayOfWeek>? recurrenceDaysOfWeek,
        DateTime? recurrenceEndDate,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        RecurrenceRule? recurrence = null;
        if (recurrenceFrequency.HasValue)
        {
            recurrence = RecurrenceRule.Create(
                recurrenceFrequency.Value,
                recurrenceInterval,
                recurrenceDaysOfWeek,
                recurrenceEndDate);
        }

        var bill = Bill.Create(projectId, title, amount, dueDate, userId, currency, description, category, recurrence);

        if (splitUserIds is { Count: > 0 })
        {
            foreach (var splitUserId in splitUserIds)
            {
                if (!project.IsMember(splitUserId))
                    throw new BusinessRuleValidationException($"User {splitUserId} is not a member of this project.");
            }

            bill.SetEqualSplit(splitUserIds, userId);
        }

        await billRepository.AddAsync(bill, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return bill;
    }

    public async Task<long> UpdateBillAsync(
        long billId,
        long userId,
        string? title,
        string? description,
        string? category,
        decimal? amount,
        DateTime? dueDate,
        CancellationToken cancellationToken = default)
    {
        var bill = await billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        bill.Update(title, description, category, amount, dueDate, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return bill.ProjectId;
    }

    public async Task<long> DeleteBillAsync(
        long billId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var bill = await billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var projectId = bill.ProjectId;

        billRepository.Remove(bill);
        await unitOfWork.CompleteAsync(cancellationToken);

        return projectId;
    }

    public async Task<IReadOnlyList<Bill>> GetByProjectAsync(
        long projectId,
        long userId,
        bool includePaid,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return await billRepository.GetByProjectAsync(projectId, includePaid, cancellationToken);
    }

    public async Task<(long ProjectId, long? NextBillId)> MarkBillPaidAsync(
        long billId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var bill = await billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var nextBill = bill.MarkFullyPaid(userId);

        if (nextBill is not null)
            await billRepository.AddAsync(nextBill, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return (bill.ProjectId, nextBill?.Id);
    }

    public async Task<long> RecordSplitPaymentAsync(
        long billId,
        long splitUserId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var bill = await billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        bill.RecordSplitPayment(splitUserId, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return bill.ProjectId;
    }

    public async Task<long> SetBillSplitAsync(
        long billId,
        long userId,
        IReadOnlyCollection<long>? equalSplitUserIds,
        IReadOnlyCollection<(long UserId, decimal Amount)>? customShares,
        CancellationToken cancellationToken = default)
    {
        var bill = await billRepository.GetByIdAsync(billId, cancellationToken)
            ?? throw new NotFoundException(nameof(Bill), billId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (customShares is { Count: > 0 })
        {
            foreach (var (shareUserId, _) in customShares)
            {
                if (!bill.Project.IsMember(shareUserId))
                    throw new BusinessRuleValidationException($"User {shareUserId} is not a member of this project.");
            }

            bill.SetCustomSplit(customShares.Select(s => (s.UserId, s.Amount)).ToList(), userId);
        }
        else if (equalSplitUserIds is { Count: > 0 })
        {
            foreach (var splitUserId in equalSplitUserIds)
            {
                if (!bill.Project.IsMember(splitUserId))
                    throw new BusinessRuleValidationException($"User {splitUserId} is not a member of this project.");
            }

            bill.SetEqualSplit(equalSplitUserIds, userId);
        }
        else
        {
            throw new BusinessRuleValidationException("Either EqualSplitUserIds or CustomShares must be provided.");
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return bill.ProjectId;
    }
}
