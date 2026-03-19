using MediatR;

namespace Feezbow.Application.Features.Projects.GetProjectMembersById;

public record GetProjectMembersByIdQuery(long ProjectId): IRequest<List<GetProjectMembersByIdQueryResponse>>;

