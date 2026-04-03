using Feezbow.Application.Common.Interfaces;

namespace Feezbow.Integration.Tests.Infrastructure;

/// <summary>
/// No-op email service for integration tests. Prevents real SMTP calls.
/// </summary>
public class FakeEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetUrl,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendPasswordResetConfirmationEmailAsync(string toEmail, string userName,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendEmailConfirmationEmailAsync(string toEmail, string userName, string confirmationUrl,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendTaskDueReminderEmailAsync(string toEmail, string userName, string taskTitle, DateTime dueDate,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
