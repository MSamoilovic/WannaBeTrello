using System.Text.RegularExpressions;

namespace Feezbow.Domain.Common;

public static partial class MentionParser
{
    [GeneratedRegex(@"@([\w.-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex MentionRegex();

    public static IReadOnlySet<string> ParseUsernames(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new HashSet<string>();

        return MentionRegex()
            .Matches(content)
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
