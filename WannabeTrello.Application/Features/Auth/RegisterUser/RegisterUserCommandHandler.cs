using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Exceptions;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Infrastructure.Services;

namespace WannabeTrello.Application.Features.Auth.RegisterUser;

public class RegisterUserCommandHandler(IUserService userService, UserManager<User> userManager, IJwtTokenService jwtTokenService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IEmailService emailService)
    : IRequestHandler<RegisterUserCommand, RegisterUserCommandResponse>
{
    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = userService.CreateUserForAuth(request.UserName, request.Email);

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
        }

        await userManager.AddToRoleAsync(user, "User");

  
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Tracking

        //TODO: Transfer to user service to request EmailCofirmation
        var ipAddress = currentUserService.UserIPAddress;
        user.RequestEmailConfirmation(ipAddress);
        await unitOfWork.CompleteAsync(cancellationToken);

        // Build confirmation URL
        var confirmationUrl = BuildConfirmationUrl(request.Email, confirmationToken);

        // Slanje confirmation email-a
        await emailService.SendEmailConfirmationEmailAsync(
            user.Email!,
            user.DisplayName,
            confirmationUrl,
            cancellationToken);

        
        var token = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        return new RegisterUserCommandResponse(
            Token: token,
            Email: user.Email!,
            EmailConfirmed: user.EmailConfirmed);
    }

    private static string BuildConfirmationUrl(string email, string token)
    {
        // TODO: Dodati FRONTENDURL u konfiguraciju
        var frontendUrl = ""; // configuration["FrontendUrl"]
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);

        return $"{frontendUrl}/confirm-email?email={encodedEmail}&token={encodedToken}";
    }
}