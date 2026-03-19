using MediatR;

namespace Feezbow.Application.Features.Projects.GetBoardsByProjectId;

public record GetBoardsByProjectIdQuery(long ProjectId): IRequest<List<GetBoardsByProjectIdQueryResponse>>;