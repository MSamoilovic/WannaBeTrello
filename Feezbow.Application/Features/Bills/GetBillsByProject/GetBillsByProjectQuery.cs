using MediatR;

namespace Feezbow.Application.Features.Bills.GetBillsByProject;

public record GetBillsByProjectQuery(long ProjectId, bool IncludePaid = false)
    : IRequest<IReadOnlyList<BillDto>>;
