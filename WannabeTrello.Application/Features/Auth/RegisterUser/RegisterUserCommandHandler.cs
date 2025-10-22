using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Exceptions;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Infrastructure.Services;

namespace WannabeTrello.Application.Features.Auth.RegisterUser;

public class RegisterUserCommandHandler(UserManager<User> userManager, IJwtTokenService jwtTokenService)
    : IRequestHandler<RegisterUserCommand, RegisterUserCommandResponse>
{
    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User { UserName = request.UserName, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);
        
        Console.WriteLine($"User created a new account with password: { user.Id}");
        
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
        }

        await userManager.AddToRoleAsync(user, "User");
        
        var token = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);
        return new RegisterUserCommandResponse(token, user.Email);
    }
}