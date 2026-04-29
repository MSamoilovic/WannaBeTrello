using Microsoft.Extensions.Options;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class AttachmentService(
    IAttachmentRepository attachmentRepository,
    IBillRepository billRepository,
    IProjectRepository projectRepository,
    IHouseholdRepository householdRepository,
    IStorageService storageService,
    IUnitOfWork unitOfWork,
    IOptions<AttachmentPolicy> policyOptions) : IAttachmentService
{
    private readonly AttachmentPolicy _policy = policyOptions.Value;

    public async Task<Attachment> UploadAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long userId,
        string fileName,
        string contentType,
        long sizeBytes,
        Stream content,
        string? description,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new BusinessRuleValidationException("UserId must be a positive number.");

        if (sizeBytes <= 0)
            throw new BusinessRuleValidationException("File is empty.");

        if (_policy.MaxFileSizeBytes > 0 && sizeBytes > _policy.MaxFileSizeBytes)
            throw new BusinessRuleValidationException(
                $"File exceeds maximum allowed size of {_policy.MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (_policy.AllowedContentTypes.Count > 0
            && !_policy.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new BusinessRuleValidationException($"Content type '{contentType}' is not allowed.");

        await EnsureUserCanAccessOwnerAsync(ownerType, ownerId, userId, cancellationToken);

        var folder = $"attachments/{ownerType.ToString().ToLowerInvariant()}/{ownerId}";
        var storageKey = await storageService.SaveAsync(folder, fileName, content, cancellationToken);

        try
        {
            var attachment = Attachment.Create(
                ownerType, ownerId, fileName, storageKey, contentType, sizeBytes, userId, description);

            await attachmentRepository.AddAsync(attachment, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return attachment;
        }
        catch
        {
            await storageService.DeleteAsync(storageKey, CancellationToken.None);
            throw;
        }
    }

    public async Task<Attachment> GetAsync(long attachmentId, long userId, CancellationToken cancellationToken = default)
    {
        var attachment = await attachmentRepository.GetByIdAsync(attachmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Attachment), attachmentId);

        await EnsureUserCanAccessOwnerAsync(attachment.OwnerType, attachment.OwnerId, userId, cancellationToken);
        return attachment;
    }

    public async Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserCanAccessOwnerAsync(ownerType, ownerId, userId, cancellationToken);
        return await attachmentRepository.GetByOwnerAsync(ownerType, ownerId, cancellationToken);
    }

    public async Task<(Attachment Metadata, Stream Content)> OpenAsync(
        long attachmentId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await GetAsync(attachmentId, userId, cancellationToken);
        var stream = await storageService.OpenReadAsync(attachment.StorageKey, cancellationToken);
        return (attachment, stream);
    }

    public async Task DeleteAsync(long attachmentId, long userId, CancellationToken cancellationToken = default)
    {
        var attachment = await attachmentRepository.GetByIdAsync(attachmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Attachment), attachmentId);

        await EnsureUserCanAccessOwnerAsync(attachment.OwnerType, attachment.OwnerId, userId, cancellationToken);

        attachment.MarkDeleted(userId);
        attachmentRepository.Remove(attachment);
        await unitOfWork.CompleteAsync(cancellationToken);

        await storageService.DeleteAsync(attachment.StorageKey, CancellationToken.None);
    }

    public async Task DeleteByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long systemUserId,
        CancellationToken cancellationToken = default)
    {
        if (systemUserId <= 0)
            throw new BusinessRuleValidationException("SystemUserId must be a positive number.");

        var attachments = await attachmentRepository.GetByOwnerAsync(ownerType, ownerId, cancellationToken);
        if (attachments.Count == 0) return;

        foreach (var attachment in attachments)
        {
            attachment.MarkDeleted(systemUserId);
            attachmentRepository.Remove(attachment);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        foreach (var attachment in attachments)
            await storageService.DeleteAsync(attachment.StorageKey, CancellationToken.None);
    }

    private async Task EnsureUserCanAccessOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        long userId,
        CancellationToken cancellationToken)
    {
        switch (ownerType)
        {
            case AttachmentOwnerType.Bill:
            {
                var bill = await billRepository.GetByIdAsync(ownerId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Bill), ownerId);

                if (!bill.Project.IsMember(userId))
                    throw new AccessDeniedException("You are not a member of this project.");
                break;
            }
            case AttachmentOwnerType.Household:
            {
                var householdExists = await householdRepository.ExistsForProjectAsync(ownerId, cancellationToken);
                if (!householdExists)
                    throw new NotFoundException(nameof(HouseholdProfile), ownerId);

                var project = await projectRepository.GetProjectWithMembersAsync(ownerId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Project), ownerId);

                if (!project.IsMember(userId))
                    throw new AccessDeniedException("You are not a member of this household.");
                break;
            }
            default:
                throw new BusinessRuleValidationException($"Unsupported attachment owner type: {ownerType}");
        }
    }
}

/// <summary>
/// Domain-side projection of storage policy (max size, allowed MIME types). Bound from <c>Storage</c>
/// section but kept here so the domain service has no Infrastructure reference.
/// </summary>
public class AttachmentPolicy
{
    public long MaxFileSizeBytes { get; set; }
    public List<string> AllowedContentTypes { get; set; } = [];
}
