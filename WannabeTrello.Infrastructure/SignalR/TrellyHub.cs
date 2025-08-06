using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.SignalR;

public class TrellyHub(IBoardRepository boardRepository) : Hub<ITrellyHub>
{
    public async Task JoinBoardGroup(long boardId)
    {
        if (!string.IsNullOrEmpty(Context.UserIdentifier) && long.TryParse(Context.UserIdentifier, out long userLongId))
        {
            var board = await boardRepository.GetBoardWithDetailsAsync(boardId);
            if (board == null || board.BoardMembers.All(bm => bm.UserId != userLongId))
            {
                throw new HubException("Niste autorizovani da se pridružite ovoj board grupi.");
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Board-{boardId}");
        Console.WriteLine(
            $"Korisnik {Context.UserIdentifier ?? Context.ConnectionId} se pridružio grupi Board-{boardId}");
    }


    public async Task LeaveBoardGroup(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Board-{boardId}");
        Console.WriteLine(
            $"Korisnik {Context.UserIdentifier ?? Context.ConnectionId} je napustio grupu Board-{boardId}");
    }


    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Klijent povezan: {Context.ConnectionId} (Korisnik: {Context.UserIdentifier})");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(
            $"Klijent prekinuo vezu: {Context.ConnectionId} (Korisnik: {Context.UserIdentifier}) Greška: {exception?.Message}");

        await base.OnDisconnectedAsync(exception);
    }
}