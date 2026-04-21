using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository taskRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    ICommentRepository commentRepository,
    IActivityLogRepository activityTrackerRepository,
    IHouseholdRepository householdRepository,
    IShoppingListRepository shoppingListRepository,
    IHouseholdChoreRepository householdChoreRepository,
    IBillRepository billRepository) : IUnitOfWork, IDisposable
{

    // Svojstva za svaki interfejs repozitorijuma
    public IBoardRepository Boards { get; } = boardRepository;
    public IColumnRepository Columns { get; } = columnRepository;
    public IBoardTaskRepository Tasks { get; } = taskRepository;
    public IProjectRepository Projects { get; } = projectRepository;
    public IUserRepository Users { get; } = userRepository;
    public ICommentRepository Comments { get; } = commentRepository;
    public IActivityLogRepository ActivityLogs { get; } = activityTrackerRepository;
    public IHouseholdRepository Households { get; } = householdRepository;
    public IShoppingListRepository ShoppingLists { get; } = shoppingListRepository;
    public IHouseholdChoreRepository Chores { get; } = householdChoreRepository;
    public IBillRepository Bills { get; } = billRepository;

    public async Task<int> CompleteAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationDomainException("A record with the same unique value already exists.");
        }
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}