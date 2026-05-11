using CreatePdf.NET.Internal;

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
            .SaveAndOcrWithLoggingAsync("AddText_HelloWorld")
            .ConfigureAwait(true);

        ocrResult.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Hello World", "OCR should accurately read the text from the PDF");
    }

    [Fact]
    public async Task AddPixelText_HelloWorld_IsReadableByOcr()
    {
        var ocrResult = await Pdf.Create()
            .AddPixelText("Hello World", Dye.Blue, Dye.White, PixelTextSize.Large)
            .SaveAndOcrWithLoggingAsync("AddPixelText_HelloWorld")
            .ConfigureAwait(true);

        ocrResult.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Hello World", "OCR should accurately read bitmap text from the PDF");
    }

    [Fact]
    public async Task Load_PdfFromStream_ExtractsTextSuccessfully()
    {
        var pdfPath = await Pdf.Create()
            .AddText("Stream Test Content")
            .AddLine()
            .AddText("This PDF was loaded from a stream")
            .SaveAsync("stream_test.pdf")
            .ConfigureAwait(true);

        var pdfBytes = await File.ReadAllBytesAsync(pdfPath).ConfigureAwait(true);
        using var stream = new MemoryStream(pdfBytes);

        var extractedText =
            await Pdf.Load(stream).OcrAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);

        extractedText.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Stream Test Content")
            .And.Contain("This PDF was loaded from a stream");
    }

    [Fact]
    public async Task ProcessPdfStreamAsync_HandlesCleanupErrorsGracefully()
    {
        var pdfBytes = await File.ReadAllBytesAsync(await Pdf.Create()
                .AddText("Exception Test")
                .SaveAsync()
                .ConfigureAwait(true))
            .ConfigureAwait(true);

        using var stream = new MemoryStream(pdfBytes);

        await Pdf.Load(stream).OcrAsync(cancellationToken: CancellationToken.None).ConfigureAwait(true);
    }

    [Fact]
    public void TryDeleteDirectory_ExistingDirectoryWithContents_DeletesRecursively()
    {
        var tempDir = Directory.CreateTempSubdirectory("createpdf-ocr-test-");
        File.WriteAllText(Path.Combine(tempDir.FullName, "leftover.txt"), "x");

        OcrService.TryDeleteDirectory(tempDir.FullName);

        Directory.Exists(tempDir.FullName).Should().BeFalse();
    }

    [Fact]
    public void TryDeleteDirectory_NonExistentDirectory_DoesNotThrow()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var act = () => OcrService.TryDeleteDirectory(nonExistent);

        act.Should().NotThrow();
    }
}
