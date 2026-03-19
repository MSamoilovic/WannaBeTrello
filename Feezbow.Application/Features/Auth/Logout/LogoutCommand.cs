using MediatR;

namespace Feezbow.Application.Features.Auth.Logout;

public record LogoutCommand : IRequest<LogoutCommandResponse>;
