using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Projects.ArchiveProject;

public record ArchiveProjectCommandResponse(Result<long> Result);
