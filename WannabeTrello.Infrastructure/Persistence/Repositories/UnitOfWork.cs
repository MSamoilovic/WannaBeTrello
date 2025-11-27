using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository taskRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    ICommentRepository commentRepository,
    IActivityLogRepository activityTrackerRepository) : IUnitOfWork, IDisposable
{

    // Svojstva za svaki interfejs repozitorijuma
    public IBoardRepository Boards { get; } = boardRepository;
    public IColumnRepository Columns { get; } = columnRepository;
    public IBoardTaskRepository Tasks { get; } = taskRepository;
    public IProjectRepository Projects { get; } = projectRepository;
    public IUserRepository Users { get; } = userRepository;
    public ICommentRepository Comments { get; } = commentRepository;
    public IActivityLogRepository ActivityLogs { get; } = activityTrackerRepository;

    public async Task<int> CompleteAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}