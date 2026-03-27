using Feezbow.Domain.Common;

namespace Feezbow.Domain.Tests.Common;

public class MentionParserTests
{
    [Fact]
    public void ParseUsernames_WithNullContent_ShouldReturnEmptySet()
    {
        var result = MentionParser.ParseUsernames(null);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseUsernames_WithEmptyOrWhitespaceContent_ShouldReturnEmptySet(string content)
    {
        var result = MentionParser.ParseUsernames(content);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseUsernames_WithNoMentions_ShouldReturnEmptySet()
    {
        var result = MentionParser.ParseUsernames("This is a comment with no mentions.");

        Assert.Empty(result);
    }

    [Fact]
    public void ParseUsernames_WithSingleMention_ShouldReturnUsername()
    {
        var result = MentionParser.ParseUsernames("Hey @john, check this out.");

        var username = Assert.Single(result);
        Assert.Equal("john", username);
    }

    [Fact]
    public void ParseUsernames_WithMultipleMentions_ShouldReturnAllUsernames()
    {
        var result = MentionParser.ParseUsernames("@alice and @bob should review this.");

        Assert.Equal(2, result.Count);
        Assert.Contains("alice", result);
        Assert.Contains("bob", result);
    }

    [Fact]
    public void ParseUsernames_WithDuplicateMentions_ShouldReturnDistinctUsernames()
    {
        var result = MentionParser.ParseUsernames("@john did this, right @john?");

        var username = Assert.Single(result);
        Assert.Equal("john", username);
    }

    [Fact]
    public void ParseUsernames_WithDuplicateMentionsDifferentCase_ShouldDeduplicateCaseInsensitively()
    {
        var result = MentionParser.ParseUsernames("@John and @JOHN are the same person.");

        Assert.Single(result);
    }

    [Fact]
    public void ParseUsernames_WithMentionContainingDots_ShouldParseFullUsername()
    {
        var result = MentionParser.ParseUsernames("Assigned to @john.doe today.");

        var username = Assert.Single(result);
        Assert.Equal("john.doe", username);
    }

    [Fact]
    public void ParseUsernames_WithMentionContainingDashes_ShouldParseFullUsername()
    {
        var result = MentionParser.ParseUsernames("Assigned to @john-doe today.");

        var username = Assert.Single(result);
        Assert.Equal("john-doe", username);
    }

    [Fact]
    public void ParseUsernames_WithMentionAtStartOfContent_ShouldParseUsername()
    {
        var result = MentionParser.ParseUsernames("@alice please review.");

        var username = Assert.Single(result);
        Assert.Equal("alice", username);
    }

    [Fact]
    public void ParseUsernames_WithMentionAtEndOfContent_ShouldParseUsername()
    {
        var result = MentionParser.ParseUsernames("Please review @alice");

        var username = Assert.Single(result);
        Assert.Equal("alice", username);
    }

    [Fact]
    public void ParseUsernames_WithAtSignAlone_ShouldReturnEmptySet()
    {
        var result = MentionParser.ParseUsernames("Send email to user @ domain");

        Assert.Empty(result);
    }
}
