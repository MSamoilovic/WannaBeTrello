using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.UserSpecifications;
public class GetUserBoardsSpecifications: BaseSpecification<Board>
{
    public GetUserBoardsSpecifications(long userId) : base(x => x.BoardMembers.Any(x => x.UserId == userId)){}
}
