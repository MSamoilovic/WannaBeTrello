using MediatR;

namespace Feezbow.Application.Features.Users.ReactivateUser;

public record ReactivateUserCommand(long UserId): IRequest<ReactivateUserCommandResponse>;