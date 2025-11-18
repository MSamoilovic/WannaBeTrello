using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using WannabeTrello.Application.Common.Interfaces;

namespace WannabeTrello.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost not configured");
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:SmtpUsername"] ?? throw new InvalidOperationException("Email:SmtpUsername not configured");
        _smtpPassword = configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("Email:SmtpPassword not configured");
        _fromEmail = configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail not configured");
        _fromName = configuration["Email:FromName"] ?? "WannabeTrello";
    }

    public async Task SendPasswordResetConfirmationEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Password Reset Successful - WannabeTrello";
        var body = BuildPasswordResetConfirmationEmailBody(userName);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl, CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password - WannabeTrello";
        var body = BuildPasswordResetEmailBody(userName, resetUrl);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildPasswordResetEmailBody(string userName, string resetUrl)
    {
        return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; }}
                        .button {{ 
                            display: inline-block;
                            padding: 12px 24px;
                            background-color: #4CAF50;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 20px 0;
                        }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class=""content"">
                            <p>Hello {userName},</p>
                            <p>We received a request to reset your password for your WannabeTrello account.</p>
                            <p>Click the button below to reset your password:</p>
                            <p style=""text-align: center;"">
                                <a href=""{resetUrl}"" class=""button"">Reset Password</a>
                            </p>
                            <p>Or copy and paste this link into your browser:</p>
                            <p style=""word-break: break-all; color: #4CAF50;"">{resetUrl}</p>
                            <p><strong>This link will expire in 1 hour.</strong></p>
                            <p>If you didn't request this password reset, you can safely ignore this email. Your password will remain unchanged.</p>
                        </div>
                        <div class=""footer"">
                            <p>&copy; 2025 Feezbow. All rights reserved.</p>
                            <p>This is an automated message, please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>";
    }

    private static string BuildPasswordResetConfirmationEmailBody(string userName)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; }}
                    .alert {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 20px 0; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <h1>Password Reset Successful</h1>
                    </div>
                    <div class=""content"">
                        <p>Hello {userName},</p>
                        <p>Your password has been successfully reset.</p>
                        <p>You can now log in to your WannabeTrello account using your new password.</p>
                        <div class=""alert"">
                            <strong>⚠️ Security Notice:</strong>
                            <p>If you did not make this change, please contact our support team immediately.</p>
                        </div>
                        <p>For security reasons, all your existing sessions have been logged out. You will need to log in again with your new password.</p>
                    </div>
                    <div class=""footer"">
                        <p>&copy; 2025 Feezbow. All rights reserved.</p>
                        <p>This is an automated message, please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}
