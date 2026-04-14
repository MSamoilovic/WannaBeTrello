using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Household.GetHouseholdProfile;

public class GetHouseholdProfileQueryResponse
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string ShoppingDay { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static GetHouseholdProfileQueryResponse FromEntity(HouseholdProfile profile)
    {
        return new GetHouseholdProfileQueryResponse
        {
            Id = profile.Id,
            ProjectId = profile.ProjectId,
            ProjectName = profile.Project?.Name ?? string.Empty,
            Address = profile.Address,
            City = profile.City,
            Timezone = profile.Timezone,
            ShoppingDay = profile.ShoppingDay.ToString(),
            CreatedAt = profile.CreatedAt
        };
    }
}
