using MediatR;

namespace Feezbow.Application.Features.Bills.RecordSplitPayment;

public record RecordSplitPaymentCommand(long BillId, long UserId) : IRequest<RecordSplitPaymentCommandResponse>;
