using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Enums;
using MediatR;

namespace Feezbow.Application.Features.Bills.UpdateBillRecurrence;

public record UpdateBillRecurrenceCommand(
    long BillId,
    RecurrenceFrequency Frequency,
    int Interval = 1,
    IEnumerable<DayOfWeek>? DaysOfWeek = null,
    DateTime? EndDate = null) : IRequest<UpdateBillRecurrenceCommandResponse>;

public record UpdateBillRecurrenceCommandResponse(Result<long> Result);
