using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Bills.GetBillsByProject;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.GetRecurringBills;

public class GetRecurringBillsQueryHandler(
    IBillService billService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetRecurringBillsQuery, IReadOnlyList<BillDto>>
{
    public async Task<IReadOnlyList<BillDto>> Handle(
        GetRecurringBillsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var bills = await billService.GetRecurringBillsAsync(request.ProjectId, userId, cancellationToken);
        return bills.Select(BillDto.FromEntity).ToList();
    }
}
