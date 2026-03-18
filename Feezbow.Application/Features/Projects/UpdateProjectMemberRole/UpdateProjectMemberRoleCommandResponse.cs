using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;

public record UpdateProjectMemberRoleCommandResponse(Result<long> Result);