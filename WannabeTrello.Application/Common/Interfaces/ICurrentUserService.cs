namespace WannabeTrello.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    string UserIPAddress { get; }
}