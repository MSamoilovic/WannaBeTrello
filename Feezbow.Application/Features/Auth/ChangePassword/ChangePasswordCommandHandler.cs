using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    UserManager<User> userManager,
    IUserService userService,
    ICurrentUserService currentUserService,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, ChangePasswordCommandResponse>
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
            logger.LogWarning("Change password failed: incorrect old password for user {UserId}", user.Id);
            return new ChangePasswordCommandResponse(Result<long>.Fail(user.Id, "Old Password is incorrect"));
        }

        var changeResult = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            throw new ValidationException(changeResult.Errors.ToString());
        }

        user.ClearRefreshToken();
        await userManager.UpdateAsync(user);

        logger.LogInformation("Password changed successfully for user {UserId}", user.Id);

        return new ChangePasswordCommandResponse(Result<long>.Success(user.Id, "Password updated successfully"));
    }
}
