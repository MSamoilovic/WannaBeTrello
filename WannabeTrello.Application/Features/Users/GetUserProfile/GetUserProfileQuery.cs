using MediatR;

namespace WannabeTrello.Application.Features.Users.GetUserProfile;

public record GetUserProfileQuery(long UserId): IRequest<GetUserProfileQueryResponse>;

