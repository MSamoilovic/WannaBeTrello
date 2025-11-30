using MediatR;

namespace WannabeTrello.Application.Features.Activities.GetActivityByUser;

public record GetActivityByUserQuery(long UserId) : IRequest<IReadOnlyList<GetActivityByUserQueryResponse>>;

