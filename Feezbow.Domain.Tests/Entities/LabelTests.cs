using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Tests.Entities;

public class LabelTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidParameters_ShouldCreateLabelInCorrectState()
    {
        // Arrange
        const string name = "Bug";
        const string color = "#FF5733";
        const long boardId = 10L;
        const long creatorUserId = 1L;

        // Act
        var label = Label.Create(name, color, boardId, creatorUserId);

        // Assert
        Assert.NotNull(label);
        Assert.Equal(name, label.Name);
        Assert.Equal(color.ToUpperInvariant(), label.Color);
        Assert.Equal(boardId, label.BoardId);
        Assert.Equal(creatorUserId, label.CreatedBy);
    }

    [Fact]
    public void Create_ShouldTrimNameAndUppercaseColor()
    {
        var label = Label.Create("  Feature  ", "#ff5733", 1L, 1L);

        Assert.Equal("Feature", label.Name);
        Assert.Equal("#FF5733", label.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowBusinessRuleValidationException(string invalidName)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Label.Create(invalidName, "#FF5733", 1L, 1L));
    }

    [Fact]
    public void Create_WithNameExceeding50Characters_ShouldThrowBusinessRuleValidationException()
    {
        var longName = new string('A', 51);

        Assert.Throws<BusinessRuleValidationException>(() =>
            Label.Create(longName, "#FF5733", 1L, 1L));
    }

    [Fact]
    public void Create_WithNameExactly50Characters_ShouldSucceed()
    {
        var maxName = new string('A', 50);

        var label = Label.Create(maxName, "#FF5733", 1L, 1L);

        Assert.Equal(maxName, label.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyColor_ShouldThrowBusinessRuleValidationException(string invalidColor)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Label.Create("Bug", invalidColor, 1L, 1L));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("FF5733")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345")]
    [InlineData("#1234567")]
    public void Create_WithInvalidHexColor_ShouldThrowBusinessRuleValidationException(string invalidColor)
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Label.Create("Bug", invalidColor, 1L, 1L));
    }

    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#f53")]
    [InlineData("#ABC")]
    [InlineData("#aabbcc")]
    public void Create_WithValidHexColor_ShouldSucceed(string validColor)
    {
        var label = Label.Create("Bug", validColor, 1L, 1L);

        Assert.NotNull(label);
    }

    // --- Update ---

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateNameAndColor()
    {
        // Arrange
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);

        // Act
        label.Update("Feature", "#00FF00", 2L);

        // Assert
        Assert.Equal("Feature", label.Name);
        Assert.Equal("#00FF00", label.Color);
        Assert.Equal(2L, label.LastModifiedBy);
        Assert.NotNull(label.LastModifiedAt);
    }

    [Fact]
    public void Update_ShouldTrimNameAndUppercaseColor()
    {
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);

        label.Update("  Hotfix  ", "#aabbcc", 1L);

        Assert.Equal("Hotfix", label.Name);
        Assert.Equal("#AABBCC", label.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ShouldThrowBusinessRuleValidationException(string invalidName)
    {
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);

        Assert.Throws<BusinessRuleValidationException>(() =>
            label.Update(invalidName, "#FF5733", 1L));
    }

    [Fact]
    public void Update_WithNameExceeding50Characters_ShouldThrowBusinessRuleValidationException()
    {
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);
        var longName = new string('A', 51);

        Assert.Throws<BusinessRuleValidationException>(() =>
            label.Update(longName, "#FF5733", 1L));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyColor_ShouldThrowBusinessRuleValidationException(string invalidColor)
    {
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);

        Assert.Throws<BusinessRuleValidationException>(() =>
            label.Update("Bug", invalidColor, 1L));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("FF5733")]
    [InlineData("#GGGGGG")]
    public void Update_WithInvalidHexColor_ShouldThrowBusinessRuleValidationException(string invalidColor)
    {
        var label = Label.Create("Bug", "#FF5733", 1L, 1L);

        Assert.Throws<BusinessRuleValidationException>(() =>
            label.Update("Bug", invalidColor, 1L));
    }
}
