using MediatR;

namespace WannabeTrello.Application.Features.Users.DeactivateUser;

public record DeactivateUserCommand(long UserId): IRequest<DeactivateUserCommandResponse>;
