using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CreatePdf.NET.Tests;

internal interface ITestLogger
{
    void WriteLine(string message);
}

internal sealed class ConsoleTestLogger : ITestLogger
{
    public void WriteLine(string message) => Console.WriteLine(message);
}

[ExcludeFromCodeCoverage(Justification = "Test helpers")]
internal static class OcrTestExtensions
{
    public static async Task<string> SaveAndOcrWithLoggingAsync(this Document document, string testName,
        ITestLogger? output = null)
    {
        output ??= new ConsoleTestLogger();

        output.WriteLine($"\n=== {testName} ===");

        var (pdfPath, text) = await document
            .SaveAndOcrAsync(string.Create(CultureInfo.InvariantCulture, $"{testName}_{DateTime.Now:fff}"))
            .ConfigureAwait(false);

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"✓ PDF created: {pdfPath}"));
        output.WriteLine(
            string.Create(CultureInfo.InvariantCulture, $"  Size: {new FileInfo(pdfPath).Length:N0} bytes"));
        output.WriteLine("✓ OCR complete");
        output.WriteLine($"  Result: '{text}'");
        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  Length: {text.Length} chars"));

        return text;
    }
}
