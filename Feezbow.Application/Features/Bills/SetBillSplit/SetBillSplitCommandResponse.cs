using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.SetBillSplit;

public record SetBillSplitCommandResponse(Result<bool> Result);
