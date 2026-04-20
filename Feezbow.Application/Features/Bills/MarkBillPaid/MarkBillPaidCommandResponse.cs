using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.MarkBillPaid;

public record MarkBillPaidCommandResponse(Result<long?> Result);
