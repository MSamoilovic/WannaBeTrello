using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Projects.UnarchiveProject;

public record UnarchiveProjectCommandResponse(Result<long> Result);
