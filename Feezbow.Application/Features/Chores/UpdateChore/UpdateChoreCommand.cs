using Feezbow.Domain.Enums;
using MediatR;

namespace Feezbow.Application.Features.Chores.UpdateChore;

public record UpdateChoreCommand(
    long ChoreId,
    string? Title = null,
    string? Description = null,
    DateTime? DueDate = null,
    TaskPriority? Priority = null) : IRequest<UpdateChoreCommandResponse>;
