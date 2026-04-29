using Feezbow.Domain.Enums;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Attachment;

/// <summary>
/// Resolves the project id behind an attachment's owner so notifications can target the right group.
/// For <see cref="AttachmentOwnerType.Household"/> the owner id is already the project id; for
/// <see cref="AttachmentOwnerType.Bill"/> a bill lookup is required.
/// </summary>
internal static class AttachmentProjectResolver
{
    public static async Task<long?> ResolveAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        IBillRepository billRepository,
        CancellationToken cancellationToken)
    {
        return ownerType switch
        {
            AttachmentOwnerType.Household => ownerId,
            AttachmentOwnerType.Bill => (await billRepository.GetByIdAsync(ownerId, cancellationToken))?.ProjectId,
            _ => null
        };
    }
}
