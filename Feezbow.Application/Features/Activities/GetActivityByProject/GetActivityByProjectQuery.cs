using MediatR;

namespace Feezbow.Application.Features.Activities.GetActivityByProject;

public record GetActivityByProjectQuery(long ProjectId): IRequest<IReadOnlyList<GetActivityByProjectQueryResponse>>;
