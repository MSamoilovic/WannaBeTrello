using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Projects.AddProjectMember;

public record AddProjectMemberCommandResponse(Result<long> Result);
