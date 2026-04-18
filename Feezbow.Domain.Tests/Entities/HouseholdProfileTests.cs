using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Household_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Tests.Entities;

public class HouseholdProfileTests
{
    private const long ProjectId = 10L;
    private const long UserId = 5L;
    private const long MemberUserId = 7L;

    private static HouseholdProfile CreateProfile() =>
        HouseholdProfile.Create(ProjectId, UserId);

    private static ProjectMember CreateMember(long projectId, long userId) =>
        ProjectMember.Create(userId, projectId, ProjectRole.Contributor);

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ValidArguments_ReturnsInitializedProfile()
    {
        var profile = HouseholdProfile.Create(ProjectId, UserId, "Main St 1", "Belgrade");

        Assert.Equal(ProjectId, profile.ProjectId);
        Assert.Equal("Main St 1", profile.Address);
        Assert.Equal("Belgrade", profile.City);
        Assert.Equal("Europe/Belgrade", profile.Timezone);
        Assert.Equal(DayOfWeek.Saturday, profile.ShoppingDay);
        Assert.Equal(UserId, profile.CreatedBy);
        Assert.Single(profile.DomainEvents);
        Assert.IsType<HouseholdProfileCreatedEvent>(profile.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveProjectId_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdProfile.Create(0, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveCreatedBy_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            HouseholdProfile.Create(ProjectId, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_ValidMember_SetsRoleAndRaisesEvent()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.ClearDomainEvents();

        profile.AssignMemberRole(member, HouseholdRole.Adult, UserId);

        Assert.Equal(HouseholdRole.Adult, member.HouseholdRole);
        Assert.Single(profile.DomainEvents);
        var evt = Assert.IsType<HouseholdMemberRoleAssignedEvent>(profile.DomainEvents.First());
        Assert.Equal(MemberUserId, evt.MemberId);
        Assert.Equal(HouseholdRole.Adult, evt.Role);
        Assert.Null(evt.PreviousRole);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_ChangeRole_RaisesEventWithPreviousRole()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.AssignMemberRole(member, HouseholdRole.Child, UserId);
        profile.ClearDomainEvents();

        profile.AssignMemberRole(member, HouseholdRole.Adult, UserId);

        Assert.Equal(HouseholdRole.Adult, member.HouseholdRole);
        var evt = Assert.IsType<HouseholdMemberRoleAssignedEvent>(profile.DomainEvents.First());
        Assert.Equal(HouseholdRole.Child, evt.PreviousRole);
        Assert.Equal(HouseholdRole.Adult, evt.Role);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_SameRole_NoOp()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.AssignMemberRole(member, HouseholdRole.Adult, UserId);
        profile.ClearDomainEvents();

        profile.AssignMemberRole(member, HouseholdRole.Adult, UserId);

        Assert.Empty(profile.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_NullMember_Throws()
    {
        var profile = CreateProfile();

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.AssignMemberRole(null!, HouseholdRole.Adult, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_NonPositiveAssignedBy_Throws()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.AssignMemberRole(member, HouseholdRole.Adult, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void AssignMemberRole_WrongProject_Throws()
    {
        var profile = CreateProfile();
        var member = CreateMember(999L, MemberUserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.AssignMemberRole(member, HouseholdRole.Adult, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveMemberRole_HasRole_ClearsAndRaisesEvent()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.AssignMemberRole(member, HouseholdRole.Child, UserId);
        profile.ClearDomainEvents();

        profile.RemoveMemberRole(member, UserId);

        Assert.Null(member.HouseholdRole);
        Assert.Single(profile.DomainEvents);
        var evt = Assert.IsType<HouseholdMemberRoleRemovedEvent>(profile.DomainEvents.First());
        Assert.Equal(MemberUserId, evt.MemberId);
        Assert.Equal(HouseholdRole.Child, evt.PreviousRole);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveMemberRole_NoRole_NoOp()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.ClearDomainEvents();

        profile.RemoveMemberRole(member, UserId);

        Assert.Empty(profile.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveMemberRole_NullMember_Throws()
    {
        var profile = CreateProfile();

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.RemoveMemberRole(null!, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveMemberRole_NonPositiveRemovedBy_Throws()
    {
        var profile = CreateProfile();
        var member = CreateMember(ProjectId, MemberUserId);
        profile.AssignMemberRole(member, HouseholdRole.Adult, UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.RemoveMemberRole(member, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RemoveMemberRole_WrongProject_Throws()
    {
        var profile = CreateProfile();
        var member = CreateMember(999L, MemberUserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            profile.RemoveMemberRole(member, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_ChangesFields_AndRaisesEvent()
    {
        var profile = CreateProfile();
        profile.ClearDomainEvents();

        profile.Update("New Address", "Novi Sad", "Europe/London", DayOfWeek.Monday, UserId);

        Assert.Equal("New Address", profile.Address);
        Assert.Equal("Novi Sad", profile.City);
        Assert.Equal("Europe/London", profile.Timezone);
        Assert.Equal(DayOfWeek.Monday, profile.ShoppingDay);
        Assert.Single(profile.DomainEvents);
        Assert.IsType<HouseholdProfileUpdatedEvent>(profile.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_NoChanges_NoOp()
    {
        var profile = HouseholdProfile.Create(ProjectId, UserId, "Addr", "City", "Europe/Belgrade", DayOfWeek.Saturday);
        profile.ClearDomainEvents();

        profile.Update("Addr", "City", "Europe/Belgrade", DayOfWeek.Saturday, UserId);

        Assert.Empty(profile.DomainEvents);
    }
}
