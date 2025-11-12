using MediatR;

namespace WannabeTrello.Application.Features.Users.UpdateUserProfile;

public class UpdateUserProfileCommand: IRequest<UpdateUserProfileCommandResponse>
{
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }

}
