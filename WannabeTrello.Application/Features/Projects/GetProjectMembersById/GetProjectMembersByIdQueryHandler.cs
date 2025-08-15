using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Projects.GetProjectMembersById;

public class
    GetProjectMembersByIdQueryHandler(
        IProjectRepository projectRepository
    ) : IRequestHandler<GetProjectMembersByIdQuery,
    List<GetProjectMembersByIdQueryResponse>>
{
    public async Task<List<GetProjectMembersByIdQueryResponse>> Handle(GetProjectMembersByIdQuery request,
        CancellationToken cancellationToken)
    {
        var projectMembers = await projectRepository.GetProjectMembersByIdAsync(request.ProjectId);
        
        return projectMembers.Select(pm => new GetProjectMembersByIdQueryResponse(
                pm.User.Id,
                pm.User.FirstName,
                pm.User.LastName,
                pm.Role
            )).ToList();
    }
}