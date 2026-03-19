using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Projects.UpdateProjectMemberRole;

public record UpdateProjectMemberRoleCommandResponse(Result<long> Result);