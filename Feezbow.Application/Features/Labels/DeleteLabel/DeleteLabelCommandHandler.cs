using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Labels.DeleteLabel;

public class DeleteLabelCommandHandler(
    ILabelRepository labelRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<DeleteLabelCommand, DeleteLabelCommandResponse>
{
    public async Task<DeleteLabelCommandResponse> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var label = await labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
        if (label is null)
            throw new KeyNotFoundException($"Label {request.LabelId} not found.");

        var boardId = label.BoardId;

        labelRepository.Delete(label);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.BoardLabels(boardId), cancellationToken);

        return new DeleteLabelCommandResponse(true, "Label deleted successfully.");
    }
}
