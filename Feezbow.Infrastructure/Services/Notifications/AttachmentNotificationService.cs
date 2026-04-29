using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Enums;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class AttachmentNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<AttachmentNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IAttachmentNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyUploaded(
        long attachmentId, long projectId, AttachmentOwnerType ownerType, long ownerId,
        string fileName, long sizeBytes, long uploadedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .AttachmentUploaded(new AttachmentUploadedNotification
            {
                AttachmentId = attachmentId,
                ProjectId = projectId,
                OwnerType = ownerType,
                OwnerId = ownerId,
                FileName = fileName,
                SizeBytes = sizeBytes,
                UploadedBy = uploadedBy
            })), cancellationToken);

    public Task NotifyDeleted(
        long attachmentId, long projectId, AttachmentOwnerType ownerType, long ownerId,
        long deletedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .AttachmentDeleted(new AttachmentDeletedNotification
            {
                AttachmentId = attachmentId,
                ProjectId = projectId,
                OwnerType = ownerType,
                OwnerId = ownerId,
                DeletedBy = deletedBy
            })), cancellationToken);
}
