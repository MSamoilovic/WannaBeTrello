using MediatR;

namespace Feezbow.Application.Features.Users.GetUserProfile;

public record GetUserProfileQuery(long UserId): IRequest<GetUserProfileQueryResponse>;

