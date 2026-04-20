using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Bills.GetBillsByProject;

public class GetBillsByProjectQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetBillsByProjectQuery, IReadOnlyList<BillDto>>
{
    public async Task<IReadOnlyList<BillDto>> Handle(
        GetBillsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (request.IncludePaid)
        {
            var all = await unitOfWork.Bills.GetByProjectAsync(request.ProjectId, true, cancellationToken);
            return all.Select(BillDto.FromEntity).ToList();
        }

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectBills(request.ProjectId),
            async () =>
            {
                var bills = await unitOfWork.Bills.GetByProjectAsync(request.ProjectId, false, cancellationToken);
                return bills.Select(BillDto.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<BillDto>();
    }
}
