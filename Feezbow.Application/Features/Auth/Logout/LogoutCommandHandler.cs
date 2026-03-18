using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    UserManager<User> userManager,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, LogoutCommandResponse>
{
    public async Task<LogoutCommandResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return new LogoutCommandResponse(false, "User not found.");

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
            return new LogoutCommandResponse(false, "User not found.");

        user.ClearRefreshToken();
        await unitOfWork.CompleteAsync(cancellationToken);

        return new LogoutCommandResponse(true, "Logged out successfully.");
    }
}
