using MediatR;

namespace Feezbow.Application.Features.Household.RemoveHouseholdRole;

public record RemoveHouseholdRoleCommand(
    long ProjectId,
    long MemberId) : IRequest<RemoveHouseholdRoleCommandResponse>;
