using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.Options;

namespace WannabeTrello.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options) => _options = options.Value;

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

    public async Task SendEmailConfirmationEmailAsync(string toEmail, string userName, string confirmationUrl, CancellationToken cancellationToken = default)
    {
        var subject = "Confirm Your Email - WannabeTrello";
        var body = BuildEmailConfirmationEmailBody(userName, confirmationUrl);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
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

    private static string BuildEmailConfirmationEmailBody(string userName, string confirmationUrl)
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
                            <h1>Confirm Your Email Address</h1>
                        </div>
                        <div class=""content"">
                            <p>Hello {userName},</p>
                            <p>Thank you for registering with WannabeTrello!</p>
                            <p>Please confirm your email address by clicking the button below:</p>
                            <p style=""text-align: center;"">
                                <a href=""{confirmationUrl}"" class=""button"">Confirm Email</a>
                            </p>
                            <p>Or copy and paste this link into your browser:</p>
                            <p style=""word-break: break-all; color: #4CAF50;"">{confirmationUrl}</p>
                            <p><strong>This link will expire in 24 hours.</strong></p>
                            <p>If you didn't create an account with WannabeTrello, you can safely ignore this email.</p>
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
