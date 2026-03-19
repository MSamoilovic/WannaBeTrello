using MediatR;

namespace Feezbow.Application.Features.Auth.RegisterUser;

public record RegisterUserCommand(string UserName, string Email, string Password)
    : IRequest<RegisterUserCommandResponse>;
