using MediatR;

namespace Feezbow.Application.Features.Users.GetCurrentUserProfile;

public record GetCurrentUserProfileQuery: IRequest<GetCurrentUserProfileQueryResponse>;
