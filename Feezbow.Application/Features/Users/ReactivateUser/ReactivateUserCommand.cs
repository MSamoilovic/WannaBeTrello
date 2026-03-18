using MediatR;

namespace WannabeTrello.Application.Features.Users.ReactivateUser;

public record ReactivateUserCommand(long UserId): IRequest<ReactivateUserCommandResponse>;