using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Users.UpdateUserProfile;

public record UpdateUserProfileCommandResponse(Result<long> Result);
