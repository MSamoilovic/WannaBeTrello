namespace WannabeTrello.Application.Features.Projects.GetBoardsByProjectId;

public record GetBoardsByProjectIdQueryResponse(long BoardId, string? BoardName);