using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IColumnRepository : IRepository<Column>
{
    Task<Column?> GetColumnDetailsByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Column?> GetColumnWithBoardAndMembersAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Column>> GetColumnsByBoardIdAsync(long boardId, CancellationToken cancellationToken = default);
    Task<Column?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}