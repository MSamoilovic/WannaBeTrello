using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Services;

public interface IHouseholdService
{
    Task<HouseholdProfile> CreateProfileAsync(
        long projectId,
        long userId,
        string? address,
        string? city,
        string? timezone,
        DayOfWeek? shoppingDay,
        CancellationToken cancellationToken = default);

    Task<HouseholdProfile> UpdateProfileAsync(
        long projectId,
        long userId,
        string? address,
        string? city,
        string? timezone,
        DayOfWeek? shoppingDay,
        CancellationToken cancellationToken = default);

    Task<HouseholdProfile> GetProfileAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectMember>> GetMembersAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default);

    Task AssignRoleAsync(
        long projectId,
        long memberId,
        HouseholdRole role,
        long userId,
        CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(
        long projectId,
        long memberId,
        long userId,
        CancellationToken cancellationToken = default);
}
