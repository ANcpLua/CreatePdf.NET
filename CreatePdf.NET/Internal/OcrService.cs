namespace CreatePdf.NET.Internal;

internal sealed class OcrService
{
    private readonly IOcrProvider _provider;

    public OcrService() : this(new TesseractOcrProvider())
    {
    }

    internal OcrService(IOcrProvider provider)
    {
        _provider = provider;
    }

    public async Task<string> ProcessPdfAsync(string pdfPath, OcrOptions options,
        CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var pdfName = Path.GetFileNameWithoutExtension(pdfPath);
        var pngPath = Path.Combine(tempDir, $"{pdfName}_{Guid.NewGuid():N}.png");
        var txtPath = Path.Combine(tempDir, $"{pdfName}_{Guid.NewGuid():N}.txt");

        try
        {
            await _provider.RasterizePdfToPngAsync(pdfPath, pngPath, options, cancellationToken).ConfigureAwait(false);
            return await _provider.ExtractTextFromImageAsync(pngPath, txtPath, options, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            TryDeleteFile(pngPath);
            TryDeleteFile(txtPath);
        }
    }

    public async Task<string> ProcessPdfStreamAsync(Stream pdfStream, OcrOptions options,
        CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var pdfFileName = Path.ChangeExtension(Path.GetRandomFileName(), ".pdf");
        var pdfPath = Path.Combine(tempDir, pdfFileName);

        try
        {
            await using (var fileStream = new FileStream(pdfPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await pdfStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            return await ProcessPdfAsync(pdfPath, options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            TryDeleteFile(pdfPath);
        }
    }

    internal static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
