using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.RecordSplitPayment;

public record RecordSplitPaymentCommandResponse(Result<bool> Result);
