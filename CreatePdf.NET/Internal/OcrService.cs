using System.Diagnostics;
using CreatePdf.NET.Public;

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
        var ocrDir = FileOperations.GetUserFriendlyDirectory("ocr");
        var pdfName = Path.GetFileNameWithoutExtension(pdfPath);
        var pngPath = Path.Combine(ocrDir, $"{pdfName}.png");
        var txtPath = Path.Combine(ocrDir, $"{pdfName}.txt");

        await ConvertPdfToPngAsync(pdfPath, pngPath, options);
        return await PerformOcrAsync(pngPath, txtPath, options);
    }
}