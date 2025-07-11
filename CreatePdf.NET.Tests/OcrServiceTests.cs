using AwesomeAssertions;
using CreatePdf.NET.Internal;
using CreatePdf.NET.Public;
using Xunit.Abstractions;

namespace CreatePdf.NET.Tests;

[Collection("OcrTests")]
[CollectionDefinition("OcrTests", DisableParallelization = true)]
public class OcrServiceTests
{
    [Fact]
    public async Task AddText_HelloWorld_IsReadableByOcr()
    {
        var ocrResult = await Pdf.Create()
            .AddText("Hello World")
            .SaveAndOcr("AddText_HelloWorld");

        ocrResult.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Hello World", "OCR should accurately read the text from the PDF");
    }

    [Fact]
    public async Task AddPixelText_HelloWorld_IsReadableByOcr()
    {
        var ocrResult = await Pdf.Create()
            .AddPixelText("Hello World", Dye.Black, Dye.White, PixelTextSize.Tiny)
            .SaveAndOcr("AddPixelText_HelloWorld");

        ocrResult.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Hello World", "OCR should accurately read bitmap text from the PDF");
    }
}

public class OcrToolsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OcrToolsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GetTesseractArguments_ValidPaths_ReturnsProperArguments()
    {
        var options = new OcrOptions();
        var args = OcrTools.GetTesseractArguments("input.png", "output", options);
    
        _testOutputHelper.WriteLine($"Tesseract arguments: {args}");

        args.Should()
            .StartWith("\"input.png\"", "input file should be first argument")
            .And.Contain("\"output\"", "output base name should be second argument")
            .And.Contain("-l eng", "should specify English language")
            .And.EndWith("--psm 6", "should use page segmentation mode 6");
    }
}
