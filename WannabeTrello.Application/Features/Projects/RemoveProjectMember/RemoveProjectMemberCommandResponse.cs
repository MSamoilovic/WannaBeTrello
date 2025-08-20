using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Projects.RemoveProjectMember;

public record RemoveProjectMemberCommandResponse(Result<long> Result);
