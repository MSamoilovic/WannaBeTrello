using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.UpdateBill;

public record UpdateBillCommandResponse(Result<bool> Result);
