using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Labels.UpdateLabel;

public class UpdateLabelCommandHandler(
    ILabelRepository labelRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<UpdateLabelCommand, UpdateLabelCommandResponse>
{
    public async Task<UpdateLabelCommandResponse> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var label = await labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
        if (label is null)
            throw new KeyNotFoundException($"Label {request.LabelId} not found.");

        label.Update(request.Name, request.Color, currentUserService.UserId.Value);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.BoardLabels(label.BoardId), cancellationToken);

        return new UpdateLabelCommandResponse(label.Id, label.Name, label.Color, label.BoardId);
    }
}
