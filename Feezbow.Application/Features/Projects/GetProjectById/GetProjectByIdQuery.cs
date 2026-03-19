using MediatR;

namespace Feezbow.Application.Features.Projects.GetProjectById;

public record GetProjectByIdQuery(long ProjectId): IRequest<GetProjectByIdQueryResponse>;
