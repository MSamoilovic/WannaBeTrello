using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class BillNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<BillNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IBillNotificationService
{
    public Task NotifyBillCreated(long billId, long projectId, string title, decimal amount, string currency, long createdBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BillCreated(new BillCreatedNotification
            {
                BillId = billId,
                ProjectId = projectId,
                Title = title,
                Amount = amount,
                Currency = currency,
                CreatedBy = createdBy
            })), cancellationToken);

    public Task NotifyBillUpdated(long billId, long projectId, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BillUpdated(new BillUpdatedNotification
            {
                BillId = billId,
                ProjectId = projectId,
                ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyBillPaid(long billId, long projectId, decimal amount, long paidBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BillPaid(new BillPaidNotification
            {
                BillId = billId,
                ProjectId = projectId,
                Amount = amount,
                PaidBy = paidBy
            })), cancellationToken);

    public Task NotifyBillSplitPaid(long billId, long projectId, long userId, decimal amount, long paidBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BillSplitPaid(new BillSplitPaidNotification
            {
                BillId = billId,
                ProjectId = projectId,
                UserId = userId,
                Amount = amount,
                PaidBy = paidBy
            })), cancellationToken);

    public Task NotifyBillDeleted(long billId, long projectId, long deletedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BillDeleted(new BillDeletedNotification
            {
                BillId = billId,
                ProjectId = projectId,
                DeletedBy = deletedBy
            })), cancellationToken);
}
