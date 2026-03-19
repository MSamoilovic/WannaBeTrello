using MediatR;

namespace Feezbow.Application.Features.Users.SearchUsers;

public record SearchUsersQuery: IRequest<IQueryable<SearchUsersQueryResponse>>;
