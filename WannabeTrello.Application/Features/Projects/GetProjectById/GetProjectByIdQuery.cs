using MediatR;

namespace WannabeTrello.Application.Features.Projects.GetProjectById;

public record GetProjectByIdQuery(long ProjectId): IRequest<GetProjectByIdQueryResponse>;
