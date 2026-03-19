using MediatR;

namespace Feezbow.Application.Features.Activities.GetActivityByUser;

public record GetActivityByUserQuery(long UserId) : IRequest<IReadOnlyList<GetActivityByUserQueryResponse>>;

