using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Users.SearchUsers;

public class SearchUsersQueryHandler(IUserService userService, ICurrentUserService currentUserService) : IRequestHandler<SearchUsersQuery, IQueryable<SearchUsersQueryResponse>>
{
    public Task<IQueryable<SearchUsersQueryResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
             throw new UnauthorizedAccessException("User is not authenticated");
        }

        var users = userService.SearchUsers();

        if (users == null) {
            throw new Exception("No users were found!");
        }
        
        var result = users.Select(SearchUsersQueryResponse.FromEntity);

        return Task.FromResult(result.AsQueryable());
    }
}
