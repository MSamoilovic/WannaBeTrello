using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Projects.CreateProject;

public record CreateProjectCommandResponse(Result<long> Result);