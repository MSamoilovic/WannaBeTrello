using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class BoardService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IBoardService
{
    public async Task<Board?> GetBoardWithDetailsAsync(long boardId, CancellationToken cancellationToken = default)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
        
        return board ?? throw new NotFoundException(nameof(Board), boardId);
    }

    public async Task<Board> CreateBoardAsync(string name, string? description, long projectId, long createdByUserId,
        CancellationToken cancellationToken = default)
    {
        // Validacija da li korisnik postoji
        var user = await userRepository.GetByIdAsync(createdByUserId);
        if (user == null)
            throw new NotFoundException(nameof(User), createdByUserId);

        // Validacija da li projekat postoji
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new NotFoundException(nameof(Project), projectId);

        // Provera da li je korisnik član projekta
        if (!project.IsMember(createdByUserId))
            throw new AccessDeniedException("Samo članovi projekta mogu kreirati board-ove.");

        // Kreiranje board-a kroz Project agregat (bolja praksa)
        var board = project.CreateBoard(name, description, createdByUserId);
        
        // Dodavanje korisnika kao admin člana board-a
        board.AddMember(user, BoardRole.Admin, createdByUserId);

        // Čuvanje promena
        await unitOfWork.CompleteAsync(cancellationToken);

        return board;
    }

    public async Task<Board> UpdateBoardAsync(long boardId, string name, string? description, long userId,
        CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om (potrebno za update)
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera permisija - samo board članovi mogu menjati board
        var isMember = await boardRepository.IsBoardMemberAsync(boardId, userId, cancellationToken);
        if (!isMember)
            throw new AccessDeniedException("Samo članovi board-a mogu menjati board detalje.");

        // Update kroz domensku logiku
        board.UpdateDetails(name, description, userId);

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);

        return board;
    }

    public async Task<bool> DeleteBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera permisija - samo admin može brisati board
        var member = board.BoardMembers.FirstOrDefault(bm => bm.UserId == userId);
        if (member == null || member.Role != BoardRole.Admin)
            throw new AccessDeniedException("Samo admin može trajno obrisati board.");

        // Hard delete board-a
        boardRepository.Delete(board);
        await unitOfWork.CompleteAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<Board>> GetBoardsByProjectAsync(long projectId, long userId, 
        CancellationToken cancellationToken = default)
    {
        // Provera da li projekat postoji
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new NotFoundException(nameof(Project), projectId);

        // Provera da li je korisnik član projekta
        if (!project.IsMember(userId))
            throw new AccessDeniedException("Samo članovi projekta mogu videti board-ove projekta.");

        // Dohvatanje svih board-ova projekta
        var boards = await boardRepository.GetBoardsByProjectIdAsync(projectId, cancellationToken);

        return boards;
    }

    public async Task<long> ArchiveBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera permisija - samo admin može arhivirati board (logika je u domenskoj metodi)
        board.Archive(userId);

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);

        return boardId;
    }

    public async Task<long> RestoreBoardAsync(long boardId, long userId, CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);
        
        board.Restore(userId);

        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);

        return boardId;
    }

    public async Task AddBoardMemberAsync(long boardId, long userId, BoardRole role, long inviterUserId,
        CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera da li korisnik postoji
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException(nameof(User), userId);

        user.EnsureActive();

        // Provera da li je inviter član board-a
        if (!board.IsMember(inviterUserId))
            throw new AccessDeniedException("Samo članovi board-a mogu dodavati nove članove.");

        // Dodavanje člana kroz domensku metodu
        board.AddMember(user, role, inviterUserId);

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task RemoveBoardMemberAsync(long boardId, long userIdToRemove, long removerUserId,
        CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera da li je remover član board-a
        if (!board.IsMember(removerUserId))
            throw new AccessDeniedException("Samo članovi board-a mogu uklanjati članove.");

        // Uklanjanje člana kroz domensku metodu
        board.RemoveMember(userIdToRemove, removerUserId);

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task UpdateBoardMemberRoleAsync(long boardId, long userId, BoardRole newRole, long updaterUserId,
        CancellationToken cancellationToken = default)
    {
        // Dohvatanje board-a sa tracking-om
        var board = await boardRepository.GetByIdWithTrackingAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera da li je updater Admin
        var updaterMember = board.BoardMembers.FirstOrDefault(bm => bm.UserId == updaterUserId);
        if (updaterMember == null || updaterMember.Role != BoardRole.Admin)
            throw new AccessDeniedException("Samo admin može menjati role članova board-a.");

        // Pronalaženje člana kojeg treba update-ovati
        var memberToUpdate = board.BoardMembers.FirstOrDefault(bm => bm.UserId == userId);
        if (memberToUpdate == null)
            throw new NotFoundException("Board Member", userId);

        // Ažuriranje role
        memberToUpdate.Role = newRole;

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Column>> GetColumnsByBoardIdAsync(long boardId, long userId,
        CancellationToken cancellationToken = default)
    {
        // Provera da li board postoji
        var board = await boardRepository.GetByIdAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        // Provera da li je korisnik član board-a
        var isMember = await boardRepository.IsBoardMemberAsync(boardId, userId, cancellationToken);
        if (!isMember)
            throw new AccessDeniedException("Samo članovi board-a mogu videti kolone board-a.");

        // Dohvatanje svih kolona board-a
        var columns = await columnRepository.GetColumnsByBoardIdAsync(boardId, cancellationToken);

        return columns;
    }

    public async Task ReorderColumnsAsync(long boardId, Dictionary<long, int> columnOrders, long userId,
        CancellationToken cancellationToken = default)
    {
        
        var board = await boardRepository.GetBoardWithColumnsAsync(boardId, cancellationToken);

        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

       
        if (!board.IsMember(userId))
            throw new AccessDeniedException("Samo članovi board-a mogu reorder-ovati kolone.");

        
        var boardColumnIds = board.Columns.Select(c => c.Id).ToHashSet();
        foreach (var columnId in columnOrders.Keys.Where(columnId => !boardColumnIds.Contains(columnId)))
        {
            throw new NotFoundException($"Column sa ID {columnId} ne pripada ovom board-u.", columnId);
        }

       
        var orders = columnOrders.Values.ToList();
        if (orders.Count != orders.Distinct().Count())
            throw new BusinessRuleValidationException("Sve kolone moraju imati jedinstvene order vrednosti.");

        // Validacija: Provera da li su order vrednosti pozitivne
        if (orders.Any(order => order <= 0))
            throw new BusinessRuleValidationException("Order vrednosti moraju biti pozitivne.");

        // Ažuriranje order-a za svaku kolonu kroz domensku logiku
        foreach (var (columnId, newOrder) in columnOrders)
        {
            var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
            if (column != null)
            {
                column.ChangeOrder(newOrder, userId);
            }
        }

        // Čuvanje promena
        boardRepository.Update(board);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Board>> GetBoardByProjectIdAsync(long projectId, long userId,
        CancellationToken cancellationToken = default)
    {
        // Provera da li projekat postoji
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new NotFoundException(nameof(Project), projectId);

        // Provera da li je korisnik član projekta
        if (!project.IsMember(userId))
            throw new AccessDeniedException("Samo članovi projekta mogu videti board-ove projekta.");

        // Dohvatanje svih board-ova projekta
        var boards = await boardRepository.GetBoardsByProjectIdAsync(projectId, cancellationToken);

        return boards;
    }
}