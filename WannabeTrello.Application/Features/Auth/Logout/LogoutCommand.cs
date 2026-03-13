using MediatR;

namespace WannabeTrello.Application.Features.Auth.Logout;

public record LogoutCommand : IRequest<LogoutCommandResponse>;
