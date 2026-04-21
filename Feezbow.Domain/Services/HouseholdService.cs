using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class HouseholdService(
    IHouseholdRepository householdRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IHouseholdService
{
    public async Task<HouseholdProfile> CreateProfileAsync(
        long projectId,
        long userId,
        string? address,
        string? city,
        string? timezone,
        DayOfWeek? shoppingDay,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (await householdRepository.ExistsForProjectAsync(projectId, cancellationToken))
            throw new InvalidOperationDomainException("This project already has a household profile.");

        var profile = HouseholdProfile.Create(projectId, userId, address, city, timezone, shoppingDay);

        await householdRepository.AddAsync(profile, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return profile;
    }

    public async Task<HouseholdProfile> UpdateProfileAsync(
        long projectId,
        long userId,
        string? address,
        string? city,
        string? timezone,
        DayOfWeek? shoppingDay,
        CancellationToken cancellationToken = default)
    {
        var profile = await householdRepository.GetByProjectIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(HouseholdProfile), projectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        profile.Update(address, city, timezone, shoppingDay, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return profile;
    }

    public async Task<HouseholdProfile> GetProfileAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await householdRepository.GetByProjectIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(HouseholdProfile), projectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return profile;
    }

    public async Task<IReadOnlyList<ProjectMember>> GetMembersAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await householdRepository.GetByProjectIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(HouseholdProfile), projectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return profile.Project.ProjectMembers.ToList();
    }

    public async Task AssignRoleAsync(
        long projectId,
        long memberId,
        HouseholdRole role,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await householdRepository.GetByProjectIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(HouseholdProfile), projectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var member = profile.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == memberId)
            ?? throw new NotFoundException(nameof(ProjectMember), memberId);

        profile.AssignMemberRole(member, role, userId);

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task RemoveRoleAsync(
        long projectId,
        long memberId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await householdRepository.GetByProjectIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(HouseholdProfile), projectId);

        if (!profile.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var member = profile.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == memberId)
            ?? throw new NotFoundException(nameof(ProjectMember), memberId);

        profile.RemoveMemberRole(member, userId);

        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
