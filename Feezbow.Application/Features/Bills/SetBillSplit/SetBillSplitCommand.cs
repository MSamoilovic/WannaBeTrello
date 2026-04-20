using MediatR;

namespace Feezbow.Application.Features.Bills.SetBillSplit;

public record BillShare(long UserId, decimal Amount);

public record SetBillSplitCommand(
    long BillId,
    IReadOnlyCollection<long>? EqualSplitUserIds = null,
    IReadOnlyCollection<BillShare>? CustomShares = null) : IRequest<SetBillSplitCommandResponse>;
