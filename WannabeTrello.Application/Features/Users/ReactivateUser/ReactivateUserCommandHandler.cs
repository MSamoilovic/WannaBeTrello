using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Users.ReactivateUser;

public class ReactivateUserCommandHandler(IUserService userService, ICurrentUserService currentUserService) : IRequestHandler<ReactivateUserCommand, ReactivateUserCommandResponse>
{
    public async Task<ReactivateUserCommandResponse> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var userId = currentUserService.UserId.Value;

        await userService.ReactivateUserAsync(request.UserId, userId, cancellationToken);

        return new ReactivateUserCommandResponse(Result<long>.Success(request.UserId, "User Reactivated Successfully"));
    }
}
