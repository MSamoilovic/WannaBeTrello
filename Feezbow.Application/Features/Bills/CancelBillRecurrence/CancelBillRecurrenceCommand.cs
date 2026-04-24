using Feezbow.Domain.Entities.Common;
using MediatR;

namespace Feezbow.Application.Features.Bills.CancelBillRecurrence;

public record CancelBillRecurrenceCommand(long BillId) : IRequest<CancelBillRecurrenceCommandResponse>;

public record CancelBillRecurrenceCommandResponse(Result<long> Result);
