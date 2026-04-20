using Feezbow.Domain.Enums;
using MediatR;

namespace Feezbow.Application.Features.Bills.CreateBill;

public record CreateBillCommand(
    long ProjectId,
    string Title,
    decimal Amount,
    DateTime DueDate,
    string Currency = "EUR",
    string? Description = null,
    string? Category = null,
    IReadOnlyCollection<long>? SplitUserIds = null,
    RecurrenceFrequency? RecurrenceFrequency = null,
    int RecurrenceInterval = 1,
    IEnumerable<DayOfWeek>? RecurrenceDaysOfWeek = null,
    DateTime? RecurrenceEndDate = null) : IRequest<CreateBillCommandResponse>;
