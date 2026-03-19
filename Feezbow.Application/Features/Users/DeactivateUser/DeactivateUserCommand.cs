using MediatR;

namespace Feezbow.Application.Features.Users.DeactivateUser;

public record DeactivateUserCommand(long UserId): IRequest<DeactivateUserCommandResponse>;
