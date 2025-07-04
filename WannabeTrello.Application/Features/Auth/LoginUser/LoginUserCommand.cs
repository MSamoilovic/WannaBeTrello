using MediatR;

namespace WannabeTrello.Application.Features.Auth.LoginUser;

public record LoginUserCommand(string UsernameOrEmail, string Password) : IRequest<LoginUserCommandResponse>;