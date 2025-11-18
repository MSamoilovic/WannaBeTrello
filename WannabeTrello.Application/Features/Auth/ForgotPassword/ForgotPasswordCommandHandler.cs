using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(UserManager<User> userManager, IEmailService emailService, IUnitOfWork unitOfWork, ICurrentUserService currentUserService): IRequestHandler<ForgotPasswordCommand, ForgotPasswordCommandResponse>
{
    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var successResponse = new ForgotPasswordCommandResponse(true, "If user exists, a password link has been sent");

        var user = await userManager.FindByEmailAsync(request.Email);

        if(user == null || !user.IsActive)
        {
            return successResponse;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var ipAddress = currentUserService.UserIPAddress;

        user.RequestPasswordReset(ipAddress);
        await unitOfWork.CompleteAsync(cancellationToken);

        var resetUrl = BuildResetUrl(request.Email, token);

        await emailService.SendPasswordResetEmailAsync(
            user.Email!,
            user.DisplayName,
            resetUrl,
            cancellationToken);

        return successResponse;
    }

    private static string BuildResetUrl(string email, string token)
    {
        //TODO: Dodati FRONTENDURL
        var frontendUrl = "";
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);

        return $"{frontendUrl}?email={encodedEmail}&token={encodedToken}";
    }
}
