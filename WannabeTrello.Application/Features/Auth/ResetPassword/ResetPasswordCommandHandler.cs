using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler(UserManager<User> userManager, IEmailService emailService, IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<ResetPasswordCommand, ResetPasswordCommandResponse>
{
    public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new BusinessRuleValidationException("Invalid password reset token.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleValidationException("Account is deactivated. Please contact support.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            throw new BusinessRuleValidationException($"Password reset failed: {errors}");
        }

        var userIpAddress = currentUserService.UserIPAddress;

        user.CompletePasswordReset(userIpAddress);
        await unitOfWork.CompleteAsync(cancellationToken);

        await emailService.SendPasswordResetConfirmationEmailAsync(
           user.Email!,
           user.DisplayName,
           cancellationToken);

        return new ResetPasswordCommandResponse(true, "Password has been reset successfully. You can now log in with your new password.");

    }
}
