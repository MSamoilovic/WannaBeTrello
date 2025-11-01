using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IBoardService
{
    Task<Board?> GetBoardWithDetailsAsync(long boardId, CancellationToken cancellationToken = default);
    
    Task<Board> CreateBoardAsync(
        string name, 
        string? description, 
        long projectId, 
        long createdByUserId,
        CancellationToken cancellationToken = default);
    
    Task<Board> UpdateBoardAsync(
        long boardId, 
        string name, 
        string? description, 
        long userId,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Board>> GetBoardsByProjectAsync(
        long projectId, 
        long userId,
        CancellationToken cancellationToken = default);
    
    Task<long> ArchiveBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default);
    
    Task<long> RestoreBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default);
    
    Task AddBoardMemberAsync(
        long boardId, 
        long userId, 
        BoardRole role, 
        long inviterUserId,
        CancellationToken cancellationToken = default);
    
    Task RemoveBoardMemberAsync(
        long boardId, 
        long userIdToRemove, 
        long removerUserId,
        CancellationToken cancellationToken = default);
    
    Task UpdateBoardMemberRoleAsync(
        long boardId, 
        long userId, 
        BoardRole newRole, 
        long updaterUserId,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Column>> GetColumnsByBoardIdAsync(
        long boardId, 
        long userId,
        CancellationToken cancellationToken = default);
}