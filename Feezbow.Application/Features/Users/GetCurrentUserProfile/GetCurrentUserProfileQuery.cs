using MediatR;

namespace WannabeTrello.Application.Features.Users.GetCurrentUserProfile;

public record GetCurrentUserProfileQuery: IRequest<GetCurrentUserProfileQueryResponse>;
