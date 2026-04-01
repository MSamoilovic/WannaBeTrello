using Microsoft.AspNetCore.Identity;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Services;

public interface IPasswordSignInService
{
    Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure);
}
