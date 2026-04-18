using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Household.GetHouseholdMembers;

public class GetHouseholdMembersQueryResponse
{
    public IReadOnlyList<HouseholdMemberDto> Members { get; set; } = [];

    public static GetHouseholdMembersQueryResponse FromMembers(IEnumerable<ProjectMember> members)
    {
        return new GetHouseholdMembersQueryResponse
        {
            Members = members.Select(m => new HouseholdMemberDto
            {
                UserId = m.UserId!.Value,
                FirstName = m.User?.FirstName,
                LastName = m.User?.LastName,
                Email = m.User?.Email,
                ProjectRole = m.Role.ToString(),
                HouseholdRole = m.HouseholdRole?.ToString()
            }).ToList()
        };
    }
}

public class HouseholdMemberDto
{
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string ProjectRole { get; set; } = string.Empty;
    public string? HouseholdRole { get; set; }
}
