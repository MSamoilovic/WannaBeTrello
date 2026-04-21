using MediatR;

namespace Feezbow.Application.Features.Bills.UpdateBill;

public record UpdateBillCommand(
    long BillId,
    string? Title = null,
    string? Description = null,
    string? Category = null,
    decimal? Amount = null,
    DateTime? DueDate = null) : IRequest<UpdateBillCommandResponse>;
