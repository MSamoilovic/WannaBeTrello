using Microsoft.Extensions.Options;

namespace WannabeTrello.Infrastructure.Options.Validators;

public class JwtOptionsValidator : IValidateOptions<JwtOptions >
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Key))
            errors.Add($"{nameof(JwtOptions.Key)} is required");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add($"{nameof(JwtOptions.Issuer)} is required");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add($"{nameof(JwtOptions.Audience)} is required");

        if (options.ExpiryMinutes < 0)
            errors.Add($"{nameof(JwtOptions.Audience)} must be greather than 0");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
