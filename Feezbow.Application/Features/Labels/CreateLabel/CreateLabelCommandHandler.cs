using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Labels.CreateLabel;

public class CreateLabelCommandHandler(
    ILabelRepository labelRepository,
    IBoardRepository boardRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<CreateLabelCommand, CreateLabelCommandResponse>
{
    public async Task<CreateLabelCommandResponse> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var board = await boardRepository.GetBoardWithDetailsAsync(request.BoardId, cancellationToken);
        if (board is null)
            throw new KeyNotFoundException($"Board {request.BoardId} not found.");

        if (!board.IsMember(currentUserService.UserId.Value))
            throw new UnauthorizedAccessException("You are not a member of this board.");

        var label = Label.Create(request.Name, request.Color, request.BoardId, currentUserService.UserId.Value);
        await labelRepository.AddAsync(label, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.BoardLabels(request.BoardId), cancellationToken);

        return new CreateLabelCommandResponse(label.Id, label.Name, label.Color, label.BoardId);
    }
}
