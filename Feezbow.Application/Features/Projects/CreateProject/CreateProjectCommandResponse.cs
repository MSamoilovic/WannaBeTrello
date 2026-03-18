using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public record CreateProjectCommandResponse(Result<long> Result);