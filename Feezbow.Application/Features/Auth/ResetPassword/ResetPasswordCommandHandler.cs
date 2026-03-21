using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, ResetPasswordCommandResponse>
{
    public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Password reset attempted with invalid token for email {Email}", request.Email);
            throw new BusinessRuleValidationException("Invalid password reset token.");
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password reset attempted for deactivated user {UserId}", user.Id);
            throw new BusinessRuleValidationException("Account is deactivated. Please contact support.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
            throw new BusinessRuleValidationException($"Password reset failed: {errors}");
        }

        var userIpAddress = currentUserService.UserIPAddress;

        user.CompletePasswordReset(userIpAddress);
        await unitOfWork.CompleteAsync(cancellationToken);

        await emailService.SendPasswordResetConfirmationEmailAsync(
           user.Email!,
           user.DisplayName,
           cancellationToken);

        logger.LogInformation("Password reset completed for user {UserId}", user.Id);

        return new ResetPasswordCommandResponse(true, "Password has been reset successfully. You can now log in with your new password.");
    }
}
