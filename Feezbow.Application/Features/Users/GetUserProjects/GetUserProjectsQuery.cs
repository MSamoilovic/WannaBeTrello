using MediatR;

namespace Feezbow.Application.Features.Users.GetUserProjects;

public record GetUserProjectsQuery(long UserId) : IRequest<GetUserProjectsQueryResponse>;

