using MediatR;

namespace WannabeTrello.Application.Features.Users.SearchUsers;

public record SearchUsersQuery: IRequest<IQueryable<SearchUsersQueryResponse>>;
