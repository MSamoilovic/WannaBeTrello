using MediatR;

namespace WannabeTrello.Application.Features.Users.GetUserProjects;

public record GetUserProjectsQuery(long UserId) : IRequest<GetUserProjectsQueryResponse>;

