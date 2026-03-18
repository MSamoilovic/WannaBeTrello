using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Infrastructure.Services;

namespace WannabeTrello.Application.Features.Auth.ConfirmEmail
{
    public class ConfirmEmailCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ConfirmEmailCommand, ConfirmEmailCommandResponse>
    {
        public async Task<ConfirmEmailCommandResponse> Handle(
            ConfirmEmailCommand request,
            CancellationToken cancellationToken)
        {
            
            var user = await userManager.FindByEmailAsync(request.Email) ?? throw new BusinessRuleValidationException("Invalid email confirmation token.");

            // Check if user is active
            if (!user.IsActive)
            {
                throw new BusinessRuleValidationException("Account is deactivated. Please contact support.");
            }

            // Check if already confirmed
            if (user.EmailConfirmed)
            {
                // User already confirmed, return success with token
                var token = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);
                return new ConfirmEmailCommandResponse(
                    Success: true,
                    Message: "Email is already confirmed.",
                    Token: token);
            }

            // Confirm email using token
            var result = await userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleValidationException($"Email confirmation failed: {errors}");
            }

            // Update user entity
            var ipAddress = currentUserService.UserIPAddress;
            user.CompleteEmailConfirmation(ipAddress);
            await unitOfWork.CompleteAsync(cancellationToken);

            // Generate JWT token
            var jwtToken = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

            return new ConfirmEmailCommandResponse(
                Success: true,
                Message: "Email confirmed successfully. You can now log in.",
                Token: jwtToken);
        }
    }
}
