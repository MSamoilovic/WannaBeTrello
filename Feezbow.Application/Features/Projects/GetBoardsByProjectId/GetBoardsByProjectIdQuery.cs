using MediatR;

namespace WannabeTrello.Application.Features.Projects.GetBoardsByProjectId;

public record GetBoardsByProjectIdQuery(long ProjectId): IRequest<List<GetBoardsByProjectIdQueryResponse>>;