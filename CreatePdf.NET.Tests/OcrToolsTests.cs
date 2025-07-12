using AwesomeAssertions;
using CreatePdf.NET.Internal;
using Xunit.Abstractions;

namespace CreatePdf.NET.Tests;

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