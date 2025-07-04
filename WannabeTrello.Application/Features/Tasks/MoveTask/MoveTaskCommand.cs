using MediatR;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

//Unit => jer se ocekuje samo da li je izvrseno ili nije
public class MoveTaskCommand : IRequest<Unit> 
{
    public long TaskId { get; set; }
    public long NewColumnId { get; set; }
}