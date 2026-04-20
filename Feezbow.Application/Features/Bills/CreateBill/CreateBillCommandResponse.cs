using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.CreateBill;

public record CreateBillCommandResponse(Result<long> Result);
