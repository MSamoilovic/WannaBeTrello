using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Household.RemoveHouseholdRole;

public record RemoveHouseholdRoleCommandResponse(Result<bool> Result);
