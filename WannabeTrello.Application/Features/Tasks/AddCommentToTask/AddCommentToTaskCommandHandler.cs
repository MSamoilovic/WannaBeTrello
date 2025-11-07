using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.AddCommentToTask;

public class AddCommentToTaskCommandHandler(IBoardTaskService boardTaskService, ICurrentUserService currentUserService)
    : IRequestHandler<AddCommentToTaskCommand, long>
{
    public async Task<long> Handle(AddCommentToTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }
        
        var comment =  await boardTaskService.AddCommentToTaskAsync(
            request.TaskId,
            currentUserService.UserId.Value,
            request.Content
        );
        
        return comment;
    }
}