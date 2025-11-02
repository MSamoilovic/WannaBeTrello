using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Specifications.ColumnSpecifications;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ColumnRepository(ApplicationDbContext dbContext) : Repository<Column>(dbContext), IColumnRepository
{
    public async Task<Column?> GetColumnDetailsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var specification = new ColumnDetailsByIdSpecification(id);
        return await GetSingleAsync(specification, cancellationToken);
    }

    public async Task<Column?> GetColumnWithBoardAndMembersAsync(long id, CancellationToken cancellationToken = default)
    {
        var specification = new ColumnWithBoardAndMembersSpecification(id);
        return await GetSingleAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Column>> GetColumnsByBoardIdAsync(long boardId, CancellationToken cancellationToken = default)
    {
        var specification = new ColumnsByBoardIdSpecification(boardId);
        return await GetAsync(specification, cancellationToken);
    }

    public new async Task<Column?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(id, cancellationToken);
    }
}