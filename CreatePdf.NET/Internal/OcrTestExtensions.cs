using System.Diagnostics.CodeAnalysis;
using CreatePdf.NET.Public;

namespace CreatePdf.NET.Internal;

internal interface ITestLogger
{
    void WriteLine(string message);
}

internal class ConsoleTestLogger : ITestLogger
{
    public void WriteLine(string message) => Console.WriteLine(message);
}

[ExcludeFromCodeCoverage]
internal static class OcrTestExtensions
{
    public static async Task<string> SaveAndOcr(this Document document, string testName, ITestLogger? output = null)
    {
        output ??= new ConsoleTestLogger();

        output.WriteLine($"\n=== {testName} ===");

        var (pdfPath, text) = await document.SaveAndOcrAsync($"{testName}_{DateTime.Now:fff}");

        output.WriteLine($"✓ PDF created: {pdfPath}");
        output.WriteLine($"  Size: {new FileInfo(pdfPath).Length:N0} bytes");
        output.WriteLine("✓ OCR complete");
        output.WriteLine($"  Result: '{text}'");
        output.WriteLine($"  Length: {text.Length} chars");

        return text;
    }
}