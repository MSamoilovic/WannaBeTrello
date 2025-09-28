using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class BoardService(
    IBoardRepository boardRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IBoardService
{
    public async Task<Board> CreateBoardAsync(long projectId, string name, string? description, long creatorUserId,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException(nameof(Project), projectId);
        }

        var projectMember = project.ProjectMembers.SingleOrDefault(pm => pm.UserId == creatorUserId);
        if (projectMember == null)
        {
            throw new AccessDeniedException("Only Project Owner or Admin can create a Board");
        }

        var board = Board.Create(name, description, projectId, creatorUserId);

        await boardRepository.AddAsync(board);
        await unitOfWork.CompleteAsync(cancellationToken);

        return board;
    }

    public async Task<List<Board>> GetBoardByProjectIdAsync(long projectId, long userId,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException(nameof(Project), projectId);
        }

        if (!project.IsMember(userId))
        {
            throw new ArgumentException("User is not member of the project");
        }

        return await boardRepository.GetBoardsByProjectIdAsync(projectId, cancellationToken);
    }

    public async Task<Board> GetBoardByIdAsync(long boardId, long userId, CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
        if (board is null)
            throw new NotFoundException(nameof(Board), boardId);

        // if (!board.IsMember(boardId))
        //     throw new AccessDeniedException("You don't have access to this board");

        return board;
    }

    public async Task<Board> UpdateBoardDetailsAsync(long boardId, string? newName, string? newDescription,
        long modifierUserId)
    {
        var board = await boardRepository.GetByIdAsync(boardId);
        if (board == null)
        {
            throw new NotFoundException(nameof(Board), boardId);
        }

        var boardMember =
            board.BoardMembers.SingleOrDefault(bm => bm.UserId == modifierUserId && bm.Role == BoardRole.Editor);
        if (boardMember == null)
        {
            throw new AccessDeniedException("You don't have permission to update this board.");
        }

        board.UpdateDetails(newName, newDescription, modifierUserId);

        await boardRepository.UpdateAsync(board);
        await unitOfWork.CompleteAsync();

        return board;
    }

    public async Task<long> ArchiveBoardAsync(long boardId, long userId)
    {
        var board =  await boardRepository.GetByIdAsync(boardId);
        if(board is null)
            throw new NotFoundException(nameof(Board), boardId);
        
        board.Archive(userId);
        
        await boardRepository.UpdateAsync(board);
        return board.Id;
    }
    
    public async Task<long> RestoreBoardAsync(long boardId, long userId)
    {
       var board = await boardRepository.GetByIdAsync(boardId);
       if (board is null)
            throw new NotFoundException(nameof(Board), boardId);
       
       board.Restore(userId);
       await boardRepository.UpdateAsync(board);
       
       return board.Id;
    }

    public async Task AddBoardMemberAsync(long boardId, long userId, BoardRole role, long inviterUserId)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
        if (board == null) throw new NotFoundException(nameof(Board), boardId);


        var inviterBoardMember =
            board.BoardMembers.SingleOrDefault(bm => bm.UserId == inviterUserId && bm.Role == BoardRole.Editor);
        if (inviterBoardMember == null) throw new AccessDeniedException("Samo urednici table mogu dodavati članove.");

        var userToAdd = await userRepository.GetByIdAsync(userId);
        if (userToAdd == null) throw new NotFoundException(nameof(User), userId);

        board.AddMember(userToAdd, role, inviterUserId);

        await boardRepository.UpdateAsync(board);
        await unitOfWork.CompleteAsync();
    }

    public async Task RemoveBoardMemberAsync(long boardId, long userId, long removerUserId)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
        if (board == null) throw new NotFoundException(nameof(Board), boardId);


        var removerBoardMember =
            board.BoardMembers.SingleOrDefault(bm => bm.UserId == removerUserId && bm.Role == BoardRole.Editor);
        if (removerBoardMember == null)
            throw new AccessDeniedException("Nemate dozvolu za uklanjanje članova sa ove table.");


        board.RemoveMember(userId, removerUserId);

        await boardRepository.UpdateAsync(board);
        await unitOfWork.CompleteAsync();
    }

    public async Task AddColumnToBoardAsync(long boardId, string columnName, long creatorUserId)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
        if (board == null) throw new NotFoundException(nameof(Board), boardId);

        var boardMember =
            board.BoardMembers.SingleOrDefault(bm => bm.UserId == creatorUserId && bm.Role == BoardRole.Editor);
        if (boardMember == null) throw new AccessDeniedException("Nemate dozvolu za dodavanje kolona na ovu tablu.");

        // Odredi redosled nove kolone
        var nextOrder = board.Columns.Count != 0 ? board.Columns.Max(c => c.Order) + 1 : 1;


        board.AddColumn(columnName, creatorUserId);

        await boardRepository.UpdateAsync(board);
        await unitOfWork.CompleteAsync();
    }
}