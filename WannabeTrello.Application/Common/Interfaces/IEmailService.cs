namespace WannabeTrello.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(
    string toEmail,
    string userName,
    string resetUrl,
    CancellationToken cancellationToken = default);

    Task SendPasswordResetConfirmationEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default);
}
