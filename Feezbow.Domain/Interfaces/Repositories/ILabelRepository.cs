using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface ILabelRepository : IRepository<Label>
{
    Task<IReadOnlyList<Label>> GetByBoardIdAsync(long boardId, CancellationToken cancellationToken = default);
    new Task<Label?> GetByIdAsync(long labelId, CancellationToken cancellationToken = default);
}
