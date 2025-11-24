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

    Task SendEmailConfirmationEmailAsync(
        string toEmail,
        string userName,
        string confirmationUrl,
        CancellationToken cancellationToken = default);
}
