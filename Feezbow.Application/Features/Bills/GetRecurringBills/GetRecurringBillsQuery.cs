using MediatR;
using Feezbow.Application.Features.Bills.GetBillsByProject;

namespace Feezbow.Application.Features.Bills.GetRecurringBills;

public record GetRecurringBillsQuery(long ProjectId) : IRequest<IReadOnlyList<BillDto>>;
