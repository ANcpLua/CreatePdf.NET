using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class TextWrapperTests
{
    [Fact]
    public void Wrap_EmptyString_ReturnsEmptyList()
    {
        var result = TextWrapper.Wrap("", 12, 100);

        result.Should().ContainSingle()
            .Which.Should().BeEmpty();
    }

    [Fact]
    public void Wrap_WithExtremelyNarrowWidth_ReturnsAtLeastOneCharPerLine()
    {
        const string text = "WWWWW";

        var result = TextWrapper.Wrap(text, 100, 1);

        result.Should().HaveCount(5);
        result.Should().OnlyContain(line => line.Length == 1);
    }

    [Fact]
    public void Wrap_SingleCharacterTooWideForWidth_StillReturnsIt()
    {
        const string text = "W";

        var result = TextWrapper.Wrap(text, 50, 1);

        result.Should().ContainSingle()
            .Which.Should().Be("W");
    }

    [Fact]
    public void Wrap_SingleShortLine_ReturnsSingleLine()
    {
        var result = TextWrapper.Wrap("Hello", 12, 200);

        result.Should().ContainSingle()
            .Which.Should().Be("Hello");
    }

    [Fact]
    public void Wrap_LongLine_WrapsAtWordBoundary()
    {
        const string text = "This is a very long line that needs wrapping";

        var result = TextWrapper.Wrap(text, 12, 150);

        result.Should().HaveCountGreaterThan(1);
        result.Should().NotContain(line => line.EndsWith(" "), "lines should be trimmed");
        result.Should().OnlyContain(line => TextWrapper.Measure(line, 12) <= 150);
    }

    [Theory]
    [InlineData("Hello\nWorld", 2)]
    [InlineData("Line1\n\nLine3", 3)]
    [InlineData("A\nB\nC\nD", 4)]
    public void Wrap_WithNewlines_SplitsCorrectly(string input, int expectedLines)
    {
        var result = TextWrapper.Wrap(input, 12, 1000);

        result.Should().HaveCount(expectedLines);
    }

    [Fact]
    public void Wrap_VeryLongWordWithoutSpaces_BreaksHard()
    {
        const string longWord = "ThisIsAVeryLongWordWithoutAnySpaces";

        var result = TextWrapper.Wrap(longWord, 12, 100);

        result.Should().HaveCountGreaterThan(1);
        result.Should().OnlyContain(line => !string.IsNullOrEmpty(line));
    }

    [Fact]
    public void Wrap_TextWithTrailingSpaces_TrimsLines()
    {
        const string text = "Hello   \nWorld   ";

        var result = TextWrapper.Wrap(text, 12, 200);

        using (new AssertionScope())
        {
            result[0].Should().Be("Hello");
            result[1].Should().Be("World");
        }
    }

    [Fact]
    public void Wrap_EmptyLinesBetweenText_PreservesEmptyLines()
    {
        const string text = "First\n\nThird";

        var result = TextWrapper.Wrap(text, 12, 200);

        result.Should().BeEquivalentTo("First", "", "Third");
    }

    [Theory]
    [InlineData(6, 50)]
    [InlineData(24, 200)]
    [InlineData(12, 100)]
    public void Wrap_DifferentFontSizes_WrapsCorrectly(int fontSize, float maxWidth)
    {
        const string text = "The quick brown fox jumps over the lazy dog";

        var result = TextWrapper.Wrap(text, fontSize, maxWidth);

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(line => TextWrapper.Measure(line, fontSize) <= maxWidth);
    }

    [Fact]
    public void Measure_CalculatesCorrectWidth()
    {
        using (new AssertionScope())
        {
            TextWrapper.Measure("", 12).Should().Be(0);
            TextWrapper.Measure("A", 12).Should().BeApproximately(6.672f, 0.001f);
            TextWrapper.Measure("Hello", 12).Should().BeApproximately(33.36f, 0.001f);
        }
    }

    [Fact]
    public void Wrap_RealWorldExample_HandlesCorrectly()
    {
        const string text = @"This is a real-world example of text that might appear in a PDF document. 
It contains multiple sentences, various punctuation marks, and even some numbers like 123.45!";

        var result = TextWrapper.Wrap(text, 14, 400);

        result.Should().NotBeEmpty()
            .And.HaveCountGreaterThan(1)
            .And.OnlyContain(line => TextWrapper.Measure(line, 14) <= 400);
    }

    [Fact]
    public void Wrap_ExtremelyNarrowWidth_WrapsEachWord()
    {
        const string text = "One Two Three";

        var result = TextWrapper.Wrap(text, 12, 40);

        result.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Wrap_OnlySpaces_ReturnsEmptyLines()
    {
        var result = TextWrapper.Wrap("   \n   ", 12, 100);

        result.Should().BeEquivalentTo("", "");
    }
}