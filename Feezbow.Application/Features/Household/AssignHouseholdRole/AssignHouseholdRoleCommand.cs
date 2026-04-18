using Feezbow.Domain.Enums;
using MediatR;

namespace Feezbow.Application.Features.Household.AssignHouseholdRole;

public record AssignHouseholdRoleCommand(
    long ProjectId,
    long MemberId,
    HouseholdRole Role) : IRequest<AssignHouseholdRoleCommandResponse>;
