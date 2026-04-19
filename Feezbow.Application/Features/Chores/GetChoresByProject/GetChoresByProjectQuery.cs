using MediatR;

namespace Feezbow.Application.Features.Chores.GetChoresByProject;

public record GetChoresByProjectQuery(long ProjectId, bool IncludeCompleted = false)
    : IRequest<IReadOnlyList<ChoreDto>>;
