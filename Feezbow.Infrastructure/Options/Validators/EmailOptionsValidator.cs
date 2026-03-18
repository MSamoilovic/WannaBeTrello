using Microsoft.Extensions.Options;

namespace WannabeTrello.Infrastructure.Options.Validators;

public class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
            errors.Add($"{nameof(EmailOptions.SmtpHost)} is required");

        if (options.SmtpPort < 1 || options.SmtpPort > 65535)
            errors.Add($"{nameof(EmailOptions.SmtpPort)} must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(options.SmtpUsername))
            errors.Add($"{nameof(EmailOptions.SmtpUsername)} is required");

        if (string.IsNullOrWhiteSpace(options.SmtpPassword))
            errors.Add($"{nameof(EmailOptions.SmtpPassword)} is required");

        if (string.IsNullOrWhiteSpace(options.FromEmail))
            errors.Add($"{nameof(EmailOptions.FromEmail)} is required");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
