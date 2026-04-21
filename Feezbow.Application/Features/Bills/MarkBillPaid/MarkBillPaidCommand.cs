using MediatR;

namespace Feezbow.Application.Features.Bills.MarkBillPaid;

public record MarkBillPaidCommand(long BillId) : IRequest<MarkBillPaidCommandResponse>;
