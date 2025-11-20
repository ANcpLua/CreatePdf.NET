using System.Globalization;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class PageTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var page = new Page(1, Dye.White);

        using (new AssertionScope())
        {
            page.Id.Should().Be(1);
            page.HasImages.Should().BeFalse();
            page.ImageIds.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddText_AutoContrastWithDarkBackground()
    {
        var darkBackground = new Dye(0.1f, 0.1f, 0.1f);
        var page = new Page(1, darkBackground);
        var almostBlack = new Dye(0.15f, 0.15f, 0.15f);

        page.AddText("Test", 100, 100, 12, almostBlack);
        var content = page.GetContent();

        content.Should().Contain("1.000000 1.000000 1.000000 rg");
    }

    [Fact]
    public void Constructor_SetsBackgroundColor()
    {
        var page = new Page(1, Dye.Red);
        var content = page.GetContent();

        content.Should().Contain("1.000000 0.000000 0.000000 rg");
    }

    [Fact]
    public void AddText_AddsTextToPage()
    {
        var page = new Page(1, Dye.White);

        page.AddText("Hello World", 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain("(Hello World)")
            .And.Contain("0.000000 0.000000 0.000000 rg");
    }

    [Fact]
    public void AddText_WithSpecialCharacters_EscapesCorrectly()
    {
        var page = new Page(1, Dye.White);

        page.AddText("Text (with parens)", 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(@"(Text \(with parens\))");
    }

    [Fact]
    public void AddText_AutoContrastWithSimilarColors()
    {
        var page = new Page(1, Dye.White);
        var almostWhite = new Dye(0.95f, 0.95f, 0.95f);

        page.AddText("Test", 100, 100, 12, almostWhite);
        var content = page.GetContent();

        content.Should().Contain("0.000000 0.000000 0.000000 rg");
    }

    [Fact]
    public void AddText_CalculatesCorrectPdfCoordinates()
    {
        var page = new Page(1, Dye.White);

        page.AddText("Test", 100, 200, 12, Dye.Black);
        var content = page.GetContent();

        const float expectedY = Layout.PageHeight - 200 - 12;
        content.Should().Contain(string.Create(CultureInfo.InvariantCulture, $"100.00 {expectedY:F2} Td"));
    }

    [Fact]
    public void AddImage_AddsImageReference()
    {
        var page = new Page(1, Dye.White);
        var image = new ImageResource(42, 100, 50, []);

        page.AddImage(image, 10, 20, 100, 50);

        using (new AssertionScope())
        {
            page.HasImages.Should().BeTrue();
            page.ImageIds.Should().ContainSingle().Which.Should().Be(42);
            page.GetContent().Should().Contain("/Im42 Do");
        }
    }

    [Fact]
    public void AddImage_MultipleImages_TracksAllIds()
    {
        var page = new Page(1, Dye.White);
        var image1 = new ImageResource(1, 10, 10, []);
        var image2 = new ImageResource(2, 20, 20, []);

        page.AddImage(image1, 0, 0, 10, 10);
        page.AddImage(image2, 20, 20, 20, 20);

        page.ImageIds.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void GetContent_ContainsCorrectPdfStructure()
    {
        var page = new Page(1, Dye.Blue);
        page.AddText("Hello", 50, 100, 14, Dye.White);

        var content = page.GetContent();

        using (new AssertionScope())
        {
            content.Should().StartWith("0.000000 0.000000 1.000000 rg");
            content.Should().Contain("q BT");
            content.Should().Contain("ET Q");
            content.Should().Contain("/Helvetica 14.00 Tf");
        }
    }

    [Fact]
    public void AddText_EmptyString_StillAddsTextCommand()
    {
        var page = new Page(1, Dye.White);

        page.AddText("", 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain("()");
    }

    [Fact]
    public void AddImage_CalculatesCorrectTransformMatrix()
    {
        var page = new Page(1, Dye.White);
        var image = new ImageResource(1, 100, 100, []);

        page.AddImage(image, 50, 100, 200, 150);
        var content = page.GetContent();

        const float expectedY = Layout.PageHeight - 100 - 150;
        content.Should()
            .Contain(string.Create(CultureInfo.InvariantCulture, $"q 200.00 0 0 150.00 50.00 {expectedY:F2} cm"));
    }

    [Theory]
    [InlineData("Hello World", "(Hello World)")]
    [InlineData("Text (with parens)", @"(Text \(with parens\))")]
    [InlineData(@"C:\path\to\file", @"(C:\\path\\to\\file)")]
    [InlineData("Line1\nLine2", @"(Line1\nLine2)")]
    [InlineData("Tab\there", @"(Tab\there)")]
    [InlineData("Return\rhere", @"(Return\rhere)")]
    [InlineData("Mix\r\n\t()\\", @"(Mix\r\n\t\(\)\\)")]
    public void AddText_EscapesSpecialCharactersCorrectly(string input, string expectedInPdf)
    {
        var page = new Page(1, Dye.White);

        page.AddText(input, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(expectedInPdf);
    }

    [Fact]
    public void AddText_EmptyString_AddsEmptyParentheses()
    {
        var page = new Page(1, Dye.White);

        page.AddText("", 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain("()");
    }

    [Fact]
    public void AddText_OnlySpecialCharacters_EscapesAll()
    {
        var page = new Page(1, Dye.White);

        page.AddText(@"\()", 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(@"(\\\(\))");
    }

    [Theory]
    [InlineData("Hello 世界")]
    [InlineData("مرحبا بالعالم")]
    [InlineData("Привет мир")]
    [InlineData("😀 Emoji")]
    [InlineData("Café ☕")]
    public void AddText_UnicodeCharacters_PassThrough(string unicodeText)
    {
        var page = new Page(1, Dye.White);

        page.AddText(unicodeText, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain($"({unicodeText})");
    }

    [Theory]
    [InlineData('\\', @"(\\)")]
    [InlineData('(', @"(\()")]
    [InlineData(')', @"(\))")]
    [InlineData('\n', @"(\n)")]
    [InlineData('\r', @"(\r)")]
    [InlineData('\t', @"(\t)")]
    public void AddText_SingleSpecialChar_EscapesCorrectly(char input, string expectedInPdf)
    {
        var page = new Page(1, Dye.White);

        page.AddText(input.ToString(), 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(expectedInPdf);
    }

    [Fact]
    public void AddText_ConsecutiveSpecialChars_EscapesAll()
    {
        var page = new Page(1, Dye.White);

        page.AddText(@"\\\\", 100, 100, 12, Dye.Black);
        page.AddText("(())", 100, 120, 12, Dye.Black);
        page.AddText("\n\n\n", 100, 140, 12, Dye.Black);

        var content = page.GetContent();

        content.Should().Contain(@"(\\\\\\\\)")
            .And.Contain(@"(\(\(\)\))")
            .And.Contain(@"(\n\n\n)");
    }

    [Fact]
    public void AddText_LongTextWithSpecialChars_HandlesCorrectly()
    {
        var page = new Page(1, Dye.White);
        var longText = string.Concat(Enumerable.Repeat("Hello (World) \n", 100));

        page.AddText(longText, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(@"\(World\)")
            .And.Contain(@"\n");
    }

    [Fact]
    public void AddText_ComplexMixedContent_EscapesCorrectly()
    {
        var page = new Page(1, Dye.White);
        const string complex = """
                               PDF (string) with \backslash and
                               new lines	plus tabs
                               """;

        page.AddText(complex, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(@"(PDF \(string\) with \\backslash and\nnew lines\tplus tabs)");
    }

    [Theory]
    [InlineData("Price: €100", "Price: EUR100")]
    [InlineData("Euro €50 and €75", "Euro EUR50 and EUR75")]
    [InlineData("Em dash — here", "Em dash - here")]
    [InlineData("En dash – there", "En dash - there")]
    [InlineData("\u201CSmart quotes\u201D", "\"Smart quotes\"")]
    [InlineData("\u2018Single quotes\u2019", "'Single quotes'")]
    [InlineData("Wait…", "Wait...")]
    [InlineData("œuvre", "oeuvre")]
    [InlineData("Œuvre", "OEuvre")]
    [InlineData("Mix: €100 — \u201Cquote\u201D — done…", "Mix: EUR100 - \"quote\" - done...")]
    [InlineData("French: œuf, Œil", "French: oeuf, OEil")]
    [InlineData("All: € — – \u201C\u201D \u2018\u2019 … œ Œ", "All: EUR - - \"\" '' ... oe OE")]
    [InlineData("Already ASCII: Hello World!", "Already ASCII: Hello World!")]
    [InlineData("", "")]
    public void GetContent_SanitizesNonLatin1Characters(string input, string expected)
    {
        var page = new Page(1, Dye.White);

        page.AddText(input, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain($"({expected})");
    }

    [Fact]
    public void AddText_RealPdfContent_HandlesCorrectly()
    {
        var page = new Page(1, Dye.White);
        const string pdfContent = "Title: My Document (v1.0)\nAuthor: John\\Jane\nPath: C:\\Documents\\file.pdf";

        page.AddText(pdfContent, 100, 100, 12, Dye.Black);
        var content = page.GetContent();

        content.Should().Contain(@"\(v1.0\)")
            .And.Contain(@"John\\Jane")
            .And.Contain(@"C:\\Documents\\file.pdf");
    }
}
