using AwesomeAssertions;
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
            .SaveAndOcr("AddText_HelloWorld");

        ocrResult.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Hello World", "OCR should accurately read the text from the PDF");
    }

    [Fact]
    public async Task AddPixelText_HelloWorld_IsReadableByOcr()
    {
        var ocrResult = await Pdf.Create()
            .AddPixelText("Hello World", Dye.Blue, Dye.White, PixelTextSize.Large)
            .SaveAndOcr("AddPixelText_HelloWorld");

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
            .SaveAsync("stream_test.pdf");

        var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
        using var stream = new MemoryStream(pdfBytes);

        var extractedText = await Pdf.Load(stream).OcrAsync();

        extractedText.Should().NotBeNullOrWhiteSpace()
            .And.Contain("Stream Test Content")
            .And.Contain("This PDF was loaded from a stream");
    }

    [Fact] 
    public async Task ProcessPdfStreamAsync_HandlesCleanupErrorsGracefully()
    {
        var pdfBytes = await File.ReadAllBytesAsync(await Pdf.Create()
            .AddText("Exception Test")
            .SaveAsync());

        using var stream = new MemoryStream(pdfBytes);
        
        await Pdf.Load(stream).OcrAsync();
    }

    [Fact]
    public void TryDeleteFile_ExistingFile_DeletesSuccessfully()
    {
        var tempFile = Path.GetTempFileName();
        File.Exists(tempFile).Should().BeTrue();

        OcrService.TryDeleteFile(tempFile);

        File.Exists(tempFile).Should().BeFalse();
    }

    [Fact]
    public void TryDeleteFile_NonExistentFile_DoesNotThrow()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        var act = () => OcrService.TryDeleteFile(nonExistentFile);
        act.Should().NotThrow();
    }
}
