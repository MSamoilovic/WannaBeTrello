using Microsoft.Extensions.Options;

namespace Feezbow.Infrastructure.Options.Validators;

public class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.LocalPath))
            errors.Add($"{nameof(StorageOptions.LocalPath)} is required");

        if (options.MaxFileSizeMb <= 0)
            errors.Add($"{nameof(StorageOptions.MaxFileSizeMb)} must be greater than zero");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
