using MediatR;

namespace Feezbow.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenCommandResponse>;
