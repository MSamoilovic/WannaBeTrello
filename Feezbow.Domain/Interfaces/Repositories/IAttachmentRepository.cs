using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default);
    void Remove(Attachment attachment);
}
