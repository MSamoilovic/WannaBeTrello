using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.DeleteBill;

public record DeleteBillCommandResponse(Result<bool> Result);
