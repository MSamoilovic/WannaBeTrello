using MediatR;

namespace Feezbow.Application.Features.Projects.RemoveProjectMember;

public record RemoveProjectMemberCommand(long ProjectId, long UserToRemoveId)
    : IRequest<RemoveProjectMemberCommandResponse>;