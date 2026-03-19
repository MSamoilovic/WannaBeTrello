using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Users.UpdateUserProfile;

public record UpdateUserProfileCommandResponse(Result<long> Result);
