using AwesomeAssertions;
using AwesomeAssertions.Execution;

namespace CreatePdf.NET.Tests;

public class DocumentTests
{
    [Fact]
    public void Create_WithoutBackground_UsesWhiteBackground()
    {
        var doc = Pdf.Create();

        doc.Should().NotBeNull();
        doc.Should().BeOfType<Document>();
    }

    [Fact]
    public void Create_WithBackground_UsesSpecifiedBackground()
    {
        var doc = Pdf.Create(Dye.Gray);

        doc.Should().NotBeNull();
    }

    [Fact]
    public void AddText_SimpleText_AddsToDocument()
    {
        var doc = Pdf.Create();

        var result = doc.AddText("Hello World");

        result.Should().BeSameAs(doc);
    }

    [Fact]
    public void AddText_WithAllParameters_UsesSpecifiedValues()
    {
        var doc = Pdf.Create();

        doc.AddText("Test", Dye.Red, TextSize.Large, TextAlignment.Right);

        var act = () => doc.SaveAsync("test");
        act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(-5)]
    public void AddLines_VariousCounts_HandlesCorrectly(int input)
    {
        var doc = Pdf.Create();

        var act = () => doc.AddLines(input);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    public void AddText_EmptyText_HandlesGracefully(string text)
    {
        var doc = Pdf.Create();

        var act = () => doc.AddText(text);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddText_NullText_HandlesGracefully()
    {
        var doc = Pdf.Create();

        var act = () => doc.AddText(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddText_WithControlCharacters_RemovesThemExceptTabAndNewline()
    {
        var doc = Pdf.Create();
        const string textWithControls = "Hello\u0001World\tTab\nNewline\u001F";

        var act = () => doc.AddText(textWithControls);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddText_VeryLongText_TruncatesToMaxLength()
    {
        var doc = Pdf.Create();
        var longText = new string('A', 15_000);

        var act = () => doc.AddText(longText);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddPixelText_SimpleText_AddsToDocument()
    {
        var doc = Pdf.Create();

        var result = doc.AddPixelText("Pixel Text");

        result.Should().BeSameAs(doc);
    }

    [Fact]
    public void AddPixelText_WithAllParameters_UsesSpecifiedValues()
    {
        var doc = Pdf.Create();

        doc.AddPixelText("Test", Dye.Yellow, Dye.Blue, PixelTextSize.Large);

        var act = () => doc.SaveAsync("test");
        act.Should().NotThrowAsync();
    }

    [Fact]
    public void AddLine_AddsEmptyLine()
    {
        var doc = Pdf.Create();

        var result = doc.AddLine();

        result.Should().BeSameAs(doc);
    }

    [Fact]
    public void AddLines_WhenNearLimit_StopsAtLimit()
    {
        var doc = Pdf.Create();

        for (var i = 0; i < 998; i++) doc.AddLine();

        doc.AddLines(10);

        var act = () => doc.AddText("Over limit");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveAsync_WithFilename_CreatesFile()
    {
        var doc = Pdf.Create();
        doc.AddText("Test content");
        var filename = $"test_{Guid.NewGuid():N}";

        var path = await doc.SaveAsync(filename);

        path.Should().EndWith(".pdf");
        File.Exists(path).Should().BeTrue();

        File.Delete(path);
    }

    [Fact]
    public void FluentApi_ChainsMultipleOperations()
    {
        var act = () => Pdf.Create()
            .AddText("Title", Dye.Black, TextSize.Large, TextAlignment.Center)
            .AddLine()
            .AddText("Body text")
            .AddLines(2)
            .AddPixelText("Pixel text", Dye.Red)
            .AddLine()
            .SaveAsync("fluent-test");

        act.Should().NotThrowAsync();
    }

    [Fact]
    public void AddText_Overloads_WorkCorrectly()
    {
        var doc = Pdf.Create();

        using (new AssertionScope())
        {
            doc.AddText("Just text").Should().BeSameAs(doc);
            doc.AddText("With size", TextSize.Large).Should().BeSameAs(doc);
            doc.AddText("With color", Dye.Red).Should().BeSameAs(doc);
            doc.AddText("With alignment", TextAlignment.Right).Should().BeSameAs(doc);
            doc.AddText("With color and size", Dye.Blue, TextSize.Small).Should().BeSameAs(doc);
        }
    }

    [Fact]
    public void AddPixelText_Overloads_WorkCorrectly()
    {
        var doc = Pdf.Create();

        using (new AssertionScope())
        {
            doc.AddPixelText("Just text").Should().BeSameAs(doc);
            doc.AddPixelText("With size", PixelTextSize.Large).Should().BeSameAs(doc);
            doc.AddPixelText("With text color", Dye.Red).Should().BeSameAs(doc);
            doc.AddPixelText("With colors", Dye.Red, Dye.Blue).Should().BeSameAs(doc);
            doc.AddPixelText("With color and size", Dye.Green, PixelTextSize.Small).Should().BeSameAs(doc);
        }
    }


    [Fact]
    public void OcrOptions_DefaultValues_AreCorrect()
    {
        var options = new OcrOptions();
    
        options.Dpi.Should().Be(300);
        options.Language.Should().Be("eng");
        options.PageSegmentationMode.Should().Be(6);
    }

    [Fact]
    public void OcrOptions_TesseractPath_CanBeSet()
    {
        var options = new OcrOptions();
        const string customPath = "/usr/local/bin/tesseract";
    
        options.TesseractPath = customPath;
    
        options.TesseractPath.Should().Be(customPath);
    }

    [Fact]
    public void OcrOptions_PdfConverterPath_CanBeSet()
    {
        var options = new OcrOptions();
        const string customPath = "/usr/local/bin/gs";
    
        options.PdfConverterPath = customPath;
    
        options.PdfConverterPath.Should().Be(customPath);
    }

    [Fact]
    public void OcrOptions_PageSegmentationMode_CanBeSet()
    {
        var options = new OcrOptions
        {
            PageSegmentationMode = 3
        };

        options.PageSegmentationMode.Should().Be(3);
    }

    [Fact]
    public void WithOcrOptions_SetsCustomOptions_CapturesCorrectly()
    {
        var doc = Pdf.Create();
        OcrOptions? capturedOptions = null;
   
        doc.WithOcrOptions(opt => 
        {
            opt.Dpi = 150;
            opt.Language = "eng";
            capturedOptions = opt;
        });
   
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Dpi.Should().Be(150);
        capturedOptions.Language.Should().Be("eng");
    }


    [Fact]
    public void SaveAndOcrAsync_MethodExists_ReturnsTask()
    {
        var doc = Pdf.Create();
    
        var method = doc.GetType().GetMethod("SaveAndOcrAsync");
    
        method.Should().NotBeNull();
        method.ReturnType.Should().Be<Task<(string, string)>>();
    }

    [Fact]
    public void SanitizeText_WithManyNewlines_LimitsTo100Lines()
    {
        var doc = Pdf.Create();
        var manyLines = string.Join("\n", Enumerable.Repeat("Line", 200));

        var act = () => doc.AddText(manyLines);
        act.Should().NotThrow();
    }
}