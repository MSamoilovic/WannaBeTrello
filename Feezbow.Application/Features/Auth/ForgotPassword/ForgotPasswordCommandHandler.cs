using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordCommandResponse>
{
    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var successResponse = new ForgotPasswordCommandResponse(true, "If user exists, a password link has been sent");

        var user = await userManager.FindByEmailAsync(request.Email);

        if(user == null || !user.IsActive)
        {
            logger.LogDebug("Password reset requested for unknown or inactive email {Email}", request.Email);
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

        logger.LogInformation("Password reset email sent for user {UserId}", user.Id);

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
