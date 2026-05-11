using System.Diagnostics;
using System.Globalization;

namespace CreatePdf.NET.Internal;

/// <summary>
///     A concrete implementation of <see cref="IOcrProvider" /> that uses external processes (Ghostscript/SIPS and
///     Tesseract) to perform OCR.
/// </summary>
internal sealed class TesseractOcrProvider : IOcrProvider
{
    private const string SipsUniversalPath = "/usr/bin/sips";
    private const string TesseractAppleSiliconPath = "/opt/homebrew/bin/tesseract";
    private const string TesseractIntelMacPath = "/usr/local/bin/tesseract";
    private const string TesseractFallback = "tesseract";
    private const string GsWindows64 = "gswin64c";
    private const string GsWindows32 = "gswin32c";
    private const string GsUnix = "gs";

    private readonly IProcessRunner _processRunner;
    private readonly ISystemEnvironment _systemEnvironment;

    internal TesseractOcrProvider()
        : this(RuntimeSystemEnvironment.Instance, ProcessRunner.Instance)
    {
    }

    internal TesseractOcrProvider(ISystemEnvironment systemEnvironment, IProcessRunner processRunner)
    {
        ArgumentNullException.ThrowIfNull(systemEnvironment);
        ArgumentNullException.ThrowIfNull(processRunner);

        _systemEnvironment = systemEnvironment;
        _processRunner = processRunner;
    }

    /// <inheritdoc />
    public async Task RasterizePdfToPngAsync(string pdfPath, string pngPath, OcrOptions options,
        CancellationToken cancellationToken = default)
    {
        await _processRunner.RunAsync(
                CreateProcessInfo(
                    GetPdfRasterizerExecutable(options),
                    GetRasterizationArguments(pdfPath, pngPath, options)),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> ExtractTextFromImageAsync(string pngPath, string txtPath, OcrOptions options,
        CancellationToken cancellationToken = default)
    {
        // Tesseract appends ".txt" itself, so strip the extension to give it the base path.
        var outputBase = Path.Combine(
            Path.GetDirectoryName(txtPath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(txtPath));
        var actualTxtPath = outputBase + ".txt";

        var result = await _processRunner.RunAsync(
                CreateProcessInfo(
                    GetTesseractExecutable(options),
                    GetOcrArguments(pngPath, outputBase, options)),
                cancellationToken)
            .ConfigureAwait(false);

        if (!_systemEnvironment.FileExists(actualTxtPath))
        {
            var exitCode = result.ExitCode.ToString(CultureInfo.InvariantCulture);
            var message = string.IsNullOrWhiteSpace(result.StandardError)
                ? $"OCR output file not found (Tesseract exited with code {exitCode})."
                : $"OCR output file not found (Tesseract exited with code {exitCode}): {result.StandardError.Trim()}";
            throw new FileNotFoundException(message, actualTxtPath);
        }

        var text = await File.ReadAllTextAsync(actualTxtPath, cancellationToken).ConfigureAwait(false);
        return text.Trim().Replace("\n", " ").Replace("\r", " ");
    }

    internal string GetPdfRasterizerExecutable(OcrOptions options)
    {
        if (!string.IsNullOrEmpty(options.PdfConverterPath))
            return options.PdfConverterPath;

        if (_systemEnvironment.IsMacOS)
            return SipsUniversalPath;

        if (_systemEnvironment.IsWindows)
            return _systemEnvironment.Is64BitOperatingSystem ? GsWindows64 : GsWindows32;

        return GsUnix;
    }

    internal string GetTesseractExecutable(OcrOptions options)
    {
        if (!string.IsNullOrEmpty(options.TesseractPath))
            return options.TesseractPath;

        return _systemEnvironment.IsMacOS switch
        {
            true when _systemEnvironment.FileExists(TesseractAppleSiliconPath) => TesseractAppleSiliconPath,
            true when _systemEnvironment.FileExists(TesseractIntelMacPath) => TesseractIntelMacPath,
            _ => TesseractFallback
        };
    }

    internal string GetRasterizationArguments(string pdfPath, string pngPath, OcrOptions options)
    {
        return _systemEnvironment.IsMacOS
            ? $"-s format png -s dpiHeight {options.Dpi.ToString(CultureInfo.InvariantCulture)} -s dpiWidth {options.Dpi.ToString(CultureInfo.InvariantCulture)} \"{pdfPath}\" --out \"{pngPath}\""
            : $"-dNOPAUSE -dBATCH -sDEVICE=pngalpha -dFirstPage=1 -dLastPage=1 -r{options.Dpi.ToString(CultureInfo.InvariantCulture)} -sOutputFile=\"{pngPath}\" \"{pdfPath}\" ";
    }

    private static string GetOcrArguments(string pngPath, string outputBase, OcrOptions options) =>
        $"\"{pngPath}\" \"{outputBase}\" -l {options.Language} --psm {options.PageSegmentationMode.ToString(CultureInfo.InvariantCulture)}";

    private static ProcessStartInfo CreateProcessInfo(string command, string arguments) => new()
    {
        FileName = command,
        Arguments = arguments,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
}
