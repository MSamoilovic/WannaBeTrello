using MediatR;

namespace Feezbow.Application.Features.Auth.LoginUser;

public record LoginUserCommand(string UsernameOrEmail, string Password) : IRequest<LoginUserCommandResponse>;