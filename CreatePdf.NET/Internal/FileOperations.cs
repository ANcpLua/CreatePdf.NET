using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CreatePdf.NET.Internal;

internal static partial class FileOperations
{
    [GeneratedRegex(@"[""<>|:*?\x00-\x1F/\\]")]
    private static partial Regex InvalidCharsRegex();
    
    private static readonly TimeProvider TimeProvider = TimeProvider.System;

    public static string GetOutputPath(string? userInput)
    {
        var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
        Directory.CreateDirectory(outputDir);
        
        return Path.Combine(outputDir, BuildSafeFileName(userInput));
    }

    private static string BuildSafeFileName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GenerateTimestampedFileName();

        var fileName = Path.GetFileName(input);
        
        var sanitized = InvalidCharsRegex().Replace(fileName, "_").Trim().Trim('_', '.');
        
        return string.IsNullOrWhiteSpace(sanitized) 
            ? GenerateTimestampedFileName()
            : Path.ChangeExtension(sanitized, ".pdf");
    }

    private static string GenerateTimestampedFileName() =>
        $"document_{TimeProvider.GetUtcNow().ToUnixTimeMilliseconds()}.pdf";

    [ExcludeFromCodeCoverage]
    public static void Open(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd" : OperatingSystem.IsMacOS() ? "open" : "xdg-open",
            Arguments = OperatingSystem.IsWindows() ? $"/c start \"\" \"{path}\"" : path,
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }

    [ExcludeFromCodeCoverage]
    public static void ShowDirectory(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "explorer" : OperatingSystem.IsMacOS() ? "open" : "xdg-open",
            Arguments = OperatingSystem.IsWindows() ? $"/select,\"{path}\"" : 
                       OperatingSystem.IsMacOS() ? $"-R {path}" : Path.GetDirectoryName(path)!,
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }
}