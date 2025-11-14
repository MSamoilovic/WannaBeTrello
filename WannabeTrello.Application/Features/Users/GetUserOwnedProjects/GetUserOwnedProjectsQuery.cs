using MediatR;

namespace WannabeTrello.Application.Features.Users.GetUserOwnedProjects;

public record GetUserOwnedProjectsQuery(long UserId) : IRequest<GetUserOwnedProjectsQueryResponse>;

