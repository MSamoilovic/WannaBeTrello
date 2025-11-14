using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;
public class GetUserBoardsSpecifications: BaseSpecification<Board>
{
    public GetUserBoardsSpecifications(long userId) : base(x => x.BoardMembers.Any(x => x.UserId == userId)){}
}
