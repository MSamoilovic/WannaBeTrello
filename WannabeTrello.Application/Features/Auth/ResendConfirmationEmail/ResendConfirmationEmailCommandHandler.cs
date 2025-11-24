using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Features.Auth.ResendConfirmationEmail
{
    public class ResendConfirmationCommandHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ResendConfirmationEmailCommand, ResendConfirmationEmailCommandResponse>
    {
        public async Task<ResendConfirmationEmailCommandResponse> Handle(
            ResendConfirmationEmailCommand request,
            CancellationToken cancellationToken)
        {
            // Always return success to prevent email enumeration
            var successResponse = new ResendConfirmationEmailCommandResponse(
                Success: true,
                Message: "If an account with that email exists and is not confirmed, a confirmation email has been sent.");

            // Find user
            var user = await userManager.FindByEmailAsync(request.Email);

            // If user doesn't exist, is not active, or is already confirmed, return success anyway
            if (user == null || !user.IsActive || user.EmailConfirmed)
            {
                return successResponse;
            }

            // Generate new confirmation token
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Update tracking
            var ipAddress = currentUserService.UserIPAddress;
            user.RequestEmailConfirmation(ipAddress);
            await unitOfWork.CompleteAsync(cancellationToken);

            // Build confirmation URL
            var confirmationUrl = BuildConfirmationUrl(request.Email, confirmationToken);

            // Send email
            await emailService.SendEmailConfirmationEmailAsync(
                user.Email!,
                user.DisplayName,
                confirmationUrl,
                cancellationToken);

            return successResponse;
        }

        private static string BuildConfirmationUrl(string email, string token)
        {
            // TODO: Dodati FRONTENDURL u konfiguraciju
            var frontendUrl = "";
            var encodedEmail = Uri.EscapeDataString(email);
            var encodedToken = Uri.EscapeDataString(token);

            return $"{frontendUrl}/confirm-email?email={encodedEmail}&token={encodedToken}";
        }
    }
}
