using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.SignalR;

public class TrellyHub() : Hub<ITrellyHub>
{
        public async Task JoinBoardGroup(string boardId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Board-{boardId}");
            
        }

      
        public async Task LeaveBoardGroup(string boardId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Board-{boardId}");
           
        }

       

        
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Klijent povezan: {Context.ConnectionId} (Korisnik: {Context.UserIdentifier})");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Klijent prekinuo vezu: {Context.ConnectionId} (Korisnik: {Context.UserIdentifier}) Greška: {exception?.Message}");
     
            await base.OnDisconnectedAsync(exception);
        }

        
    }
