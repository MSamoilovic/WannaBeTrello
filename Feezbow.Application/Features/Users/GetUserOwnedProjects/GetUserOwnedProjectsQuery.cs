using MediatR;

namespace Feezbow.Application.Features.Users.GetUserOwnedProjects;

public record GetUserOwnedProjectsQuery(long UserId) : IRequest<GetUserOwnedProjectsQueryResponse>;

