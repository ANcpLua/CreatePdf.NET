namespace CreatePdf.NET.Internal;

internal sealed class OcrService
{
    private const string TempDirPrefix = "createpdf-ocr-";

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
        var workDir = Directory.CreateTempSubdirectory(TempDirPrefix);
        try
        {
            return await OcrAsync(pdfPath, workDir.FullName, options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            TryDeleteDirectory(workDir.FullName);
        }
    }

    public async Task<string> ProcessPdfStreamAsync(Stream pdfStream, OcrOptions options,
        CancellationToken cancellationToken = default)
    {
        var workDir = Directory.CreateTempSubdirectory(TempDirPrefix);
        try
        {
            var pdfPath = Path.Combine(workDir.FullName, "input.pdf");
            await using (var fileStream =
                         new FileStream(pdfPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await pdfStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            return await OcrAsync(pdfPath, workDir.FullName, options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            TryDeleteDirectory(workDir.FullName);
        }
    }

    private async Task<string> OcrAsync(string pdfPath, string workDir, OcrOptions options,
        CancellationToken cancellationToken)
    {
        var pdfName = Path.GetFileNameWithoutExtension(pdfPath);
        var pngPath = Path.Combine(workDir, $"{pdfName}.png");
        var txtPath = Path.Combine(workDir, $"{pdfName}.txt");

        await _provider.RasterizePdfToPngAsync(pdfPath, pngPath, options, cancellationToken).ConfigureAwait(false);
        return await _provider.ExtractTextFromImageAsync(pngPath, txtPath, options, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static void TryDeleteDirectory(string path, Action<string, bool>? deleteImpl = null)
    {
        try
        {
            if (!Directory.Exists(path)) return;
            (deleteImpl ?? Directory.Delete)(path, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best-effort cleanup. Called from finally — must not mask the original exception.
        }
    }
}
