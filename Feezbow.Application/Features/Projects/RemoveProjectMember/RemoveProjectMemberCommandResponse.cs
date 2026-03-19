using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Projects.RemoveProjectMember;

public record RemoveProjectMemberCommandResponse(Result<long> Result);
