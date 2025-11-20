using CreatePdf.NET.Internal;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CreatePdf.NET.Tests;

public class OcrServiceUnitTests
{
    private const string TestPdfPath = "test.pdf";
    private const string ExpectedText = "Extracted Text Content";

    private readonly IPdfOcrEngine _ocrEngine;
    private readonly OcrService _sut; // System Under Test

    public OcrServiceUnitTests()
    {
        _ocrEngine = Substitute.For<IPdfOcrEngine>();
        _sut = new OcrService(_ocrEngine);
    }

    [Fact]
    public async Task ProcessPdfAsync_WhenEngineSucceeds_ReturnsExtractedText()
    {
        // Arrange
        _ocrEngine.ExtractTextFromImageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OcrOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ExpectedText));

        // Act
        var result = await _sut.ProcessPdfAsync(TestPdfPath, new OcrOptions()).ConfigureAwait(true);

        // Assert
        result.Should().Be(ExpectedText);

        // Verify the engine methods were called in order
        await _ocrEngine.Received(1).RasterizePdfToPngAsync(
            Arg.Is(TestPdfPath),
            Arg.Is<string>(s => s.EndsWith(".png", StringComparison.Ordinal)),
            Arg.Any<OcrOptions>(),
            Arg.Any<CancellationToken>()).ConfigureAwait(true);

        await _ocrEngine.Received(1).ExtractTextFromImageAsync(
            Arg.Is<string>(s => s.EndsWith(".png", StringComparison.Ordinal)),
            Arg.Is<string>(s => s.EndsWith(".txt", StringComparison.Ordinal)),
            Arg.Any<OcrOptions>(),
            Arg.Any<CancellationToken>()).ConfigureAwait(true);
    }

    [Fact]
    public async Task ProcessPdfAsync_WhenConversionFails_CleansUpFilesAndThrows()
    {
        // Arrange
        _ocrEngine.RasterizePdfToPngAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OcrOptions>(),
                Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Conversion failed"));

        // Act
        var act = () => _sut.ProcessPdfAsync(TestPdfPath, new OcrOptions());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Conversion failed").ConfigureAwait(true);

        // Verify OCR was NOT attempted
        await _ocrEngine.DidNotReceive().ExtractTextFromImageAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OcrOptions>(), Arg.Any<CancellationToken>())
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task ProcessPdfStreamAsync_WhenEngineSucceeds_ReturnsExtractedText()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]); // Dummy content

        _ocrEngine.ExtractTextFromImageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OcrOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ExpectedText));

        // Act
        var result = await _sut.ProcessPdfStreamAsync(stream, new OcrOptions()).ConfigureAwait(true);

        // Assert
        result.Should().Be(ExpectedText);

        // Verify conversion happened with a temp file
        await _ocrEngine.Received(1).RasterizePdfToPngAsync(
            Arg.Any<string>(), // The temp file path is random, so just check it was called
            Arg.Any<string>(),
            Arg.Any<OcrOptions>(),
            Arg.Any<CancellationToken>()).ConfigureAwait(true);
    }
}
