using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IColumnService
{
    Task<Column> CreateColumnAsync(long boardId, string? name, int order, long userId, CancellationToken cancellationToken);
    Task<Column> GetColumnByIdAsync(long boardId, long userId, CancellationToken cancellationToken);
    Task<Column> UpdateColumnAsync(long columnId, string? newName, int? wipLimit, long userId, CancellationToken cancellationToken);
    Task<long> DeleteColumnAsync(long columnId, long? userId, CancellationToken cancellationToken);
    Task ReorderColumnsAsync(long boardId, Dictionary<long, int> columnOrders, long userId, CancellationToken cancellationToken);
}