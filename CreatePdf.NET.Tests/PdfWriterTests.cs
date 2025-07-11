using System.Buffers;
using System.Text;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Internal;
using CreatePdf.NET.Public;

namespace CreatePdf.NET.Tests;

public class PdfWriterTests
{
    private static readonly Encoding Latin1 = Encoding.Latin1;

    [Fact]
    public async Task DrawText_SimpleText_WritesToPdf()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Hello World", 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("(Hello World)");
    }

    [Fact]
    public async Task DrawText_WithSpecialCharacters_EscapesCorrectly()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Text (with parens)", 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain(@"(Text \(with parens\))");
    }

    [Fact]
    public async Task DrawText_LongText_WrapsAcrossMultipleLines()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);
        var longText = string.Join(" ", Enumerable.Repeat("word", 50));

        writer.DrawText(longText, 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());

        content.Split("(word").Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task DrawText_ExceedsPageHeight_CreatesNewPage()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        for (var i = 0; i < 100; i++) writer.DrawText($"Line {i}", 12, Dye.Black, TextAlignment.Left);

        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Split("/Type /Page").Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task DrawText_LeftAlignment_PositionsAtLeftMargin()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Left", 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("(Left)")
            .And.Contain("50.00 ");
    }

    [Fact]
    public async Task DrawText_CenterAlignment_PositionsAtCenter()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Center", 12, Dye.Black, TextAlignment.Center);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("(Center)")
            .And.MatchRegex(@"2\d\d\.\d\d");
    }

    [Fact]
    public async Task DrawText_RightAlignment_PositionsAtRightMargin()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Right", 12, Dye.Black, TextAlignment.Right);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("(Right)")
            .And.MatchRegex(@"5\d\d\.\d\d");
    }

    [Fact]
    public async Task DrawBitmapText_SimpleText_CreatesImageReference()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawBitmapText("TEST", Dye.Black, Dye.White, 2);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("/Type /XObject")
            .And.Contain("/Subtype /Image")
            .And.Contain("/Im");
    }

    [Fact]
    public async Task DrawBitmapText_MultilineText_CreatesMultipleImages()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawBitmapText("Line1\nLine2", Dye.Red, Dye.Blue, 1);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());

        content.Should().Contain("/Im2 Do");
        content.Should().Contain("/Im3 Do");
    }

    [Fact]
    public async Task DrawBitmapText_LongLine_WrapsToMultipleImages()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);
        var longText = new string('A', 200);

        writer.DrawBitmapText(longText, Dye.Black, Dye.White, 1);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());

        content.Split("/Type /XObject").Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task DrawImage_AddsImageToPage()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);
        using var image = new BitmapImage(100, 50, MemoryPool<byte>.Shared.Rent(15000));

        writer.DrawImage(image);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("/Type /XObject")
            .And.Contain("/Subtype /Image")
            .And.Contain("/Width 100")
            .And.Contain("/Height 50");
    }

    [Fact]
    public async Task DrawBitmapText_WithEmptyLines_SkipsEmptyLines()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawBitmapText("Line1\n\nLine3", Dye.Black, Dye.White, 1);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());

        content.Should().Contain("/Im2 Do");
        content.Should().Contain("/Im3 Do");
        content.Should().NotContain("/Im4 Do");
    }

    [Fact]
    public async Task DrawImage_FillsPageThenCreatesNew()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        for (var i = 0; i < 41; i++) writer.DrawText($"Line {i}", 12, Dye.Black, TextAlignment.Left);

        using var image = new BitmapImage(200, 200, MemoryPool<byte>.Shared.Rent(120000));

        writer.DrawImage(image);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Split("/Type /Page").Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task FinalizeAsync_EmptyDocument_GeneratesValidPdfStructure()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.Gray);

        await writer.FinalizeAsync();

        var pdf = stream.ToArray();
        var content = Latin1.GetString(pdf);

        using (new AssertionScope())
        {
            content.Should().StartWith("%PDF-1.7");
            content.Should().EndWith("%%EOF");

            content.Should().Contain("%%Creator: CreatePdf.NET");
            content.Should().Contain("/Type /Catalog");
            content.Should().Contain("/Type /Pages");
            content.Should().Contain("/Type /Page");
            content.Should().Contain("xref");
            content.Should().Contain("trailer");
            content.Should().Contain("startxref");
        }
    }

    [Fact]
    public async Task FinalizeAsync_WithBackgroundColor_SetsPageBackground()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.Red);

        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("1.000000 0.000000 0.000000 rg");
    }

    [Fact]
    public async Task DrawText_ThenDrawImage_MaintainsCorrectOrder()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);
        using var image = new BitmapImage(10, 10, MemoryPool<byte>.Shared.Rent(300));

        writer.DrawText("Before Image", 12, Dye.Black, TextAlignment.Center);
        writer.DrawImage(image);
        writer.DrawText("After Image", 12, Dye.Black, TextAlignment.Center);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        var beforeIndex = content.IndexOf("(Before Image)", StringComparison.Ordinal);
        var imageIndex = content.IndexOf("/Im", StringComparison.Ordinal);
        var afterIndex = content.IndexOf("(After Image)", StringComparison.Ordinal);

        using (new AssertionScope())
        {
            beforeIndex.Should().BePositive();
            imageIndex.Should().BePositive();
            afterIndex.Should().BePositive();
            beforeIndex.Should().BeLessThan(imageIndex);
            imageIndex.Should().BeLessThan(afterIndex);
        }
    }

    [Fact]
    public async Task WriteXRefTable_GeneratesCorrectFormat()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Test", 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().MatchRegex(@"xref\n0 \d+\n0000000000 65535 f ");
        content.Should().MatchRegex(@"\d{10} 00000 n ");
    }

    [Fact]
    public async Task MultiplePagesWithImages_HandlesResourcesCorrectly()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 50; j++) writer.DrawText($"Page {i + 1} Line {j}", 12, Dye.Black, TextAlignment.Left);

            writer.DrawBitmapText($"PAGE{i + 1}", Dye.Black, Dye.White, 1);
        }

        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());

        content.Split("/Type /Page ").Length.Should().BeGreaterThan(2);

        content.Split("/XObject <<").Length.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task DisposeAsync_CanBeCalledMultipleTimes()
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream, Dye.White);

        var act = async () =>
        {
            await writer.DisposeAsync();
            await writer.DisposeAsync();
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DrawText_WithLatin1Characters_HandlesCorrectly()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        writer.DrawText("Café © résumé", 12, Dye.Black, TextAlignment.Left);
        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("(Caf");
        content.Should().Contain("sum");
    }

    [Fact]
    public async Task FinalizeAsync_ValidatePageDimensions()
    {
        using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, Dye.White);

        await writer.FinalizeAsync();

        var content = Latin1.GetString(stream.ToArray());
        content.Should().Contain("/MediaBox [0 0 595.00 842.00]");
    }
}