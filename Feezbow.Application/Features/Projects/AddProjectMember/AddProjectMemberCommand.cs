using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Projects.AddProjectMember;

public record AddProjectMemberCommand(
    long ProjectId,
    long NewMemberId,
    ProjectRole Role
) : IRequest<AddProjectMemberCommandResponse>;
