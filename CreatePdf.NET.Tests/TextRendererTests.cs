using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class TextRendererTests
{
    [Fact]
    public void RenderBitmap_WithEmptyText_CreatesMinimalImage()
    {
        using var image = TextRenderer.RenderBitmap("", Dye.Black, Dye.White);

        using (new AssertionScope())
        {
            image.Should().NotBeNull();
            image.Width.Should().Be(20);
            image.Height.Should().Be(56);
        }
    }

    [Fact]
    public void RenderBitmap_WithSingleCharacter_CreatesCorrectSizedImage()
    {
        using var image = TextRenderer.RenderBitmap("A", Dye.Red, Dye.Blue);

        image.Should().BeEquivalentTo(new
        {
            Width = 44,
            Height = 56
        });
    }

    [Fact]
    public void RenderBitmap_WithMultipleCharacters_AccountsForSpacing()
    {
        const string text = "ABC";

        using var image = TextRenderer.RenderBitmap(text, Dye.Black, Dye.White);

        var expectedWidth = text.Length * (TextRenderer.CharWidth + 1) * 3 - 1 * 3 + 20;
        image.Width.Should().Be(expectedWidth);
    }

    [Theory]
    [InlineData("Hello", 5)]
    [InlineData("123", 3)]
    [InlineData("!@#", 3)]
    public void RenderBitmap_OnlyRendersKnownCharacters(string input, int expectedChars)
    {
        using var image = TextRenderer.RenderBitmap(input, Dye.Black, Dye.White);

        var expectedWidth = expectedChars * (TextRenderer.CharWidth + 1) * 3 - 1 * 3 + 20;
        image.Width.Should().Be(expectedWidth);
    }

    [Fact]
    public void RenderBitmap_WithUnknownCharacters_SkipsThem()
    {
        using var image = TextRenderer.RenderBitmap("A🚀B", Dye.Black, Dye.White);

        const int expectedWidth = 2 * (TextRenderer.CharWidth + 1) * 3 - 1 * 3 + 20;
        image.Width.Should().Be(expectedWidth);
    }

    [Theory]
    [InlineData(1, 28, 32)]
    [InlineData(2, 36, 44)]
    [InlineData(5, 60, 80)]
    public void RenderBitmap_WithDifferentScales_ScalesCorrectly(int scale, int expectedWidth, int expectedHeight)
    {
        using var image = TextRenderer.RenderBitmap("A", Dye.Black, Dye.White, scale);

        image.Should().BeEquivalentTo(new
        {
            Width = expectedWidth,
            Height = expectedHeight
        });
    }

    [Fact]
    public void RenderBitmap_WithSpecialCharacters_RendersCorrectly()
    {
        var specialChars = ".,!?-_:;";

        using var image = TextRenderer.RenderBitmap(specialChars, Dye.Black, Dye.White);

        var expectedWidth = specialChars.Length * (TextRenderer.CharWidth + 1) * 3 - 1 * 3 + 20;
        image.Width.Should().Be(expectedWidth);
    }

    [Theory]
    [InlineData(0, 20, 56)]
    [InlineData(1, 44, 56)]
    [InlineData(5, 152, 56)]
    public void CalculateDimensions_VariousInputs_ReturnsCorrectValues(int charCount, int expectedWidth,
        int expectedHeight)
    {
        var (width, height) = TextRenderer.CalculateDimensions(charCount, 3);

        using (new AssertionScope())
        {
            width.Should().Be(expectedWidth);
            height.Should().Be(expectedHeight);
        }
    }

    [Fact]
    public void FindMaxFit_ForceZeroMaxChars()
    {
        const string text = "ABC";
        var result = TextWrapper.Wrap(text, 100, 1);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(line => line.Length == 1);
    }

    [Fact]
    public void RenderGlyph_FillsCorrectPixels()
    {
        const int width = 100;
        const int height = 100;
        var bitmap = new bool[width * height];
        var glyph = TextRenderer.Glyphs['I'];

        TextRenderer.RenderGlyph(bitmap, width, glyph, 20, 20, 2);

        bitmap.Should().Contain(true, "character 'I' should have visible pixels");
    }

    [Fact]
    public void RenderGlyph_WithBoundaryConditions_DoesNotThrow()
    {
        var bitmap = new bool[100];
        var glyph = TextRenderer.Glyphs['A'];

        var act = () => TextRenderer.RenderGlyph(bitmap, 10, glyph, 5, 5, 1);
        act.Should().NotThrow();
    }

    [Fact]
    public void RenderGlyph_OutOfBounds_HandlesGracefully()
    {
        var bitmap = new bool[10];
        var glyph = TextRenderer.Glyphs['A'];

        var act = () => TextRenderer.RenderGlyph(bitmap, 1, glyph, 0, 0, 5);
        act.Should().NotThrow();
    }

    [Fact]
    public void Glyphs_Dictionary_ContainsExpectedCharacters()
    {
        using (new AssertionScope())
        {
            TextRenderer.Glyphs.Should().ContainKey('A');
            TextRenderer.Glyphs.Should().ContainKey('0');
            TextRenderer.Glyphs.Should().ContainKey(' ');
            TextRenderer.Glyphs.Should().ContainKey('!');
            TextRenderer.Glyphs.Keys.Should().HaveCountGreaterThan(100);
        }
    }

    [Fact]
    public void Glyphs_AllGlyphsHaveCorrectHeight()
    {
        TextRenderer.Glyphs.Values
            .Should().OnlyContain(glyph => glyph.Length == TextRenderer.CharHeight);
    }

    [Fact]
    public void RenderBitmap_IntegrationTest_ProducesValidImageData()
    {
        const string text = "Hello World!";

        using var image = TextRenderer.RenderBitmap(text, Dye.Red, Dye.Blue, 2);

        using (new AssertionScope())
        {
            image.Should().NotBeNull();
            image.Width.Should().BePositive();
            image.Height.Should().BePositive();
            image.Size.Should().Be(image.Width * image.Height * 3);
            image.GetPixelMemory().Length.Should().Be(image.Size);
        }
    }

    [Theory]
    [InlineData("Ä", "Ä")]
    [InlineData("ñ", "ñ")]
    [InlineData("€", "€")]
    public void RenderBitmap_WithInternationalCharacters_RendersCorrectly(string input, string expectedInGlyphs)
    {
        TextRenderer.Glyphs.Should().ContainKey(expectedInGlyphs[0]);

        using var image = TextRenderer.RenderBitmap(input, Dye.Black, Dye.White);

        image.Width.Should().BeGreaterThan(20);
    }

    [Fact]
    public void RenderBitmap_PixelPerfectTest()
    {
        const char testChar = 'I';

        using var image = TextRenderer.RenderBitmap(testChar.ToString(), Dye.White, Dye.Black, 1);

        var pixels = image.GetPixelMemory().ToArray();

        pixels.Chunk(3)
            .Should().Contain(rgb => rgb.SequenceEqual(new byte[] { 255, 255, 255 }), "should have white pixels")
            .And.Contain(rgb => rgb.SequenceEqual(new byte[] { 0, 0, 0 }), "should have black pixels");
    }

    [Fact]
    public void RenderBitmap_CharacterRendering_VerifyDimensions()
    {
        var testCases = new[]
        {
            (chars: 1, scale: 1, expectedW: 28),
            (chars: 1, scale: 3, expectedW: 44),
            (chars: 3, scale: 1, expectedW: 46)
        };

        foreach (var (chars, scale, expectedW) in testCases)
        {
            var text = new string('A', chars);
            using var image = TextRenderer.RenderBitmap(text, Dye.Black, Dye.White, scale);

            image.Width.Should().Be(expectedW,
                $"for {chars} chars at scale {scale}");
        }
    }
}