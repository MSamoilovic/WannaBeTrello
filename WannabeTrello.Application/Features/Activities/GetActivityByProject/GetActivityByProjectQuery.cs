using MediatR;

namespace WannabeTrello.Application.Features.Activities.GetActivityByProject;

public record GetActivityByProjectQuery(long ProjectId): IRequest<IReadOnlyList<GetActivityByProjectQueryResponse>>;
