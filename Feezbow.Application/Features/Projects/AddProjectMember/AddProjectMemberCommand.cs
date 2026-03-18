using MediatR;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.AddProjectMember;

public record AddProjectMemberCommand(
    long ProjectId,
    long NewMemberId,
    ProjectRole Role
) : IRequest<AddProjectMemberCommandResponse>;
