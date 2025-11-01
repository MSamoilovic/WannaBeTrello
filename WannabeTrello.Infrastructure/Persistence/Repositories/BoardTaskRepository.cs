using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class BoardTaskRepository(ApplicationDbContext dbContext)
    : Repository<BoardTask>(dbContext), IBoardTaskRepository
{
    public Task<BoardTask?> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<List<BoardTask>> GetTasksByBoardIdAsync(long boardId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BoardTask>? GetTaskDetailsByIdAsync(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(BoardTask? task)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(BoardTask? task)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public IQueryable<BoardTask> SearchTask()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BoardTask>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
}