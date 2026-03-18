using MediatR;

namespace WannabeTrello.Application.Features.Projects.GetProjectMembersById;

public record GetProjectMembersByIdQuery(long ProjectId): IRequest<List<GetProjectMembersByIdQueryResponse>>;

