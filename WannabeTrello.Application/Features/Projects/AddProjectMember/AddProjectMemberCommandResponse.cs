using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Projects.AddProjectMember;

public record AddProjectMemberCommandResponse(Result<long> Result);
