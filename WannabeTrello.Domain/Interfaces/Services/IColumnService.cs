using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IColumnService
{
    Task<Column> CreateColumnAsync(long boardId, string? name, int order, long userId, CancellationToken cancellationToken);
    Task<Column> GetColumnByIdAsync(long boardId, long userId, CancellationToken cancellationToken);
}