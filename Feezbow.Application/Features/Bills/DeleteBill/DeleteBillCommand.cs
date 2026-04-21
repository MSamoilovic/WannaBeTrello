using MediatR;

namespace Feezbow.Application.Features.Bills.DeleteBill;

public record DeleteBillCommand(long BillId) : IRequest<DeleteBillCommandResponse>;
