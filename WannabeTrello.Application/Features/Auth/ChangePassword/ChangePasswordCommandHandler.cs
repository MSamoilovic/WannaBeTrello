using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler(UserManager<User> userManager, IUserService userService, ICurrentUserService currentUserService) : IRequestHandler<ChangePasswordCommand, ChangePasswordCommandResponse>
{
    public async Task<ChangePasswordCommandResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if(!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authorized");
        }

        var user = await userService.GetUserProfileAsync(currentUserService.UserId.Value, cancellationToken);

        if(user is null)
        {
            throw new NotFoundException(nameof(User), currentUserService.UserId.Value);
        }


        var isOldPasswordValid = await userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!isOldPasswordValid)
        {
            return new ChangePasswordCommandResponse(Result<long>.Fail(user.Id, "Old Password is incorrect"));
        }

        var changeResult = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            throw new ValidationException(changeResult.Errors.ToString());
        }

        return new ChangePasswordCommandResponse(Result<long>.Success(user.Id, "Password updated successfully"));

    }
}
