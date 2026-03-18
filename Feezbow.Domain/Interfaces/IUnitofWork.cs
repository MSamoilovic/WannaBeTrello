using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBoardRepository Boards { get; }
    IColumnRepository Columns { get; }
    IBoardTaskRepository Tasks { get; }
    IProjectRepository Projects { get; }
    IUserRepository Users { get; }
    ICommentRepository Comments { get; }
    IActivityLogRepository ActivityLogs { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default); // Čuva promene u bazi podataka
}