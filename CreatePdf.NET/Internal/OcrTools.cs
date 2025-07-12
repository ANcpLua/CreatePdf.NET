using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET.Internal;

internal static class OcrTools
{
    private const string SipsUniversalPath = "/usr/bin/sips";
    private const string TesseractAppleSiliconPath = "/opt/homebrew/bin/tesseract";
    private const string TesseractIntelMacPath = "/usr/local/bin/tesseract";
    private const string TesseractFallback = "tesseract";
    private const string GsWindows64 = "gswin64c";
    private const string GsWindows32 = "gswin32c";
    private const string GsUnix = "gs";

    [ExcludeFromCodeCoverage]
    public static string GetPdfToPngConverter(OcrOptions options)
    {
        if (!string.IsNullOrEmpty(options.PdfConverterPath))
            return options.PdfConverterPath;

        if (OperatingSystem.IsMacOS())
            return SipsUniversalPath;

        if (OperatingSystem.IsWindows())
            return Environment.Is64BitOperatingSystem ? GsWindows64 : GsWindows32;

        return GsUnix;
    }

    [ExcludeFromCodeCoverage]
    public static string GetTesseractPath(OcrOptions options)
    {
        if (!string.IsNullOrEmpty(options.TesseractPath))
            return options.TesseractPath;

        return OperatingSystem.IsMacOS() switch
        {
            true when File.Exists(TesseractAppleSiliconPath) => TesseractAppleSiliconPath,
            true when File.Exists(TesseractIntelMacPath) => TesseractIntelMacPath,
            _ => TesseractFallback
        };
    }

    public static string GetPdfToPngArguments(string pdfPath, string pngPath, OcrOptions options)
    {
        return OperatingSystem.IsMacOS()
            ? $"-s format png -s dpiHeight {options.Dpi} -s dpiWidth {options.Dpi} \"{pdfPath}\" --out \"{pngPath}\""
            : $"-dNOPAUSE -dBATCH -sDEVICE=pngalpha -dFirstPage=1 -dLastPage=1 -r{options.Dpi} -sOutputFile=\"{pngPath}\" \"{pdfPath}\"";
    }

    public static string GetTesseractArguments(string pngPath, string outputBase, OcrOptions options)
    {
        return $"\"{pngPath}\" \"{outputBase}\" -l {options.Language} --psm {options.PageSegmentationMode}";
    }

    public static ProcessStartInfo CreateProcessInfo(string command, string arguments) => new()
    {
        FileName = command,
        Arguments = arguments,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
}