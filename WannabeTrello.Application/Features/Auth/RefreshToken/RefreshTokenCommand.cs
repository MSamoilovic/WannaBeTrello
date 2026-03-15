using MediatR;

namespace WannabeTrello.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenCommandResponse>;
