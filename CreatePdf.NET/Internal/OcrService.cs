using System.Diagnostics;

namespace CreatePdf.NET.Internal;

internal static class OcrService
{
    private static async Task ConvertPdfToPngAsync(string pdfPath, string pngPath, OcrOptions options)
    {
        using var process = Process.Start(OcrTools.CreateProcessInfo(
            OcrTools.GetPdfToPngConverter(options),
            OcrTools.GetPdfToPngArguments(pdfPath, pngPath, options)))!;

        await process.WaitForExitAsync();
    }

    private static async Task<string> PerformOcrAsync(string pngPath, string txtPath, OcrOptions options)
    {
        var outputBase = txtPath[..^4];

        using var process = Process.Start(OcrTools.CreateProcessInfo(
            OcrTools.GetTesseractPath(options),
            OcrTools.GetTesseractArguments(pngPath, outputBase, options)))!;

        await process.WaitForExitAsync();

        var text = await File.ReadAllTextAsync(txtPath);
        return text.Trim().Replace("\n", " ").Replace("\r", " ");
    }

    public static async Task<string> ProcessPdfAsync(string pdfPath, OcrOptions options)
    {
        var tempDir = Path.GetTempPath();
        var pdfName = Path.GetFileNameWithoutExtension(pdfPath);
        var pngPath = Path.Combine(tempDir, $"{pdfName}_{Guid.NewGuid():N}.png");
        var txtPath = Path.Combine(tempDir, $"{pdfName}_{Guid.NewGuid():N}.txt");

        try
        {
            await ConvertPdfToPngAsync(pdfPath, pngPath, options);
            return await PerformOcrAsync(pngPath, txtPath, options);
        }
        finally
        {
            TryDeleteFile(pngPath);
            TryDeleteFile(txtPath);
        }
    }

    public static async Task<string> ProcessPdfStreamAsync(Stream pdfStream, OcrOptions options)
    {
        var pdfPath = Path.GetTempFileName();
        var pngPath = Path.ChangeExtension(pdfPath, ".png");
        var txtPath = Path.ChangeExtension(pdfPath, ".txt");

        try
        {
            await using (var fileStream = File.Create(pdfPath))
            {
                await pdfStream.CopyToAsync(fileStream);
            }

            await ConvertPdfToPngAsync(pdfPath, pngPath, options);
            return await PerformOcrAsync(pngPath, txtPath, options);
        }
        finally
        {
            TryDeleteFile(pdfPath);
            TryDeleteFile(pngPath);
            TryDeleteFile(txtPath);
        }
    }

    internal static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}