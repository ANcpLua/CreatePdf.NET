using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CreatePdf.NET.Internal;

internal static partial class FileOperations
{
    [GeneratedRegex(@"[""<>|:*?\x00-\x1F/\\]", RegexOptions.CultureInvariant, 500)]
    private static partial Regex InvalidCharsRegex();

    public static string GetOutputPath(string? userInput)
    {
        var outputDir = GetUserFriendlyOutputDirectory();
        Directory.CreateDirectory(outputDir);

        return Path.Combine(outputDir, BuildSafeFileName(userInput));
    }

    private static string GetUserFriendlyOutputDirectory()
    {
        var projectRoot = FindProjectRoot();

        return Path.Combine(projectRoot ?? Directory.GetCurrentDirectory(), "output");
    }

    internal static string? FindProjectRoot(string? startDirectory = null)
    {
        var current = startDirectory ?? AppContext.BaseDirectory;

        for (var dir = new DirectoryInfo(current); dir != null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.csproj").Any() || dir.EnumerateFiles("*.sln").Any())
                return dir.FullName;
        }

        return null;
    }

    private static string BuildSafeFileName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return GenerateTimestampedFileName();

        var fileName = Path.GetFileName(input);

        var sanitized = InvalidCharsRegex().Replace(fileName, "_").Trim(' ', '_', '.');

        return string.IsNullOrWhiteSpace(sanitized)
            ? GenerateTimestampedFileName()
            : Path.ChangeExtension(sanitized, ".pdf");
    }

    private static string GenerateTimestampedFileName() => string.Create(CultureInfo.InvariantCulture,
        $"document_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.pdf");

    [ExcludeFromCodeCoverage(Justification = "Interacts with OS shell")]
    public static void Open(string path)
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
#pragma warning disable ERP022 // An exit point '}' swallows an unobserved exception
        catch (Exception)
        {
            // Best-effort: Silently ignore if opening fails (missing file association, permission denied, etc.)
        }
#pragma warning restore ERP022 // An exit point '}' swallows an unobserved exception
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
    }

    [ExcludeFromCodeCoverage(Justification = "Interacts with OS shell")]
    public static void ShowDirectory(string path)
    {
        var environment = RuntimeSystemEnvironment.Instance;
        try
        {
            var (fileName, arguments) = true switch
            {
                _ when environment.IsWindows => ("explorer", $"/select,\"{path}\""),
                _ when environment.IsMacOS => ("open", $"-R \"{path}\""),
                _ => ("xdg-open", $"\"{Path.GetDirectoryName(path) ?? path}\"")
            };

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = fileName, Arguments = arguments, UseShellExecute = true, CreateNoWindow = true
            });
        }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
#pragma warning disable ERP022 // An exit point '}' swallows an unobserved exception
        catch (Exception)
        {
            // Best-effort: Silently ignore if showing directory fails
        }
#pragma warning restore ERP022 // An exit point '}' swallows an unobserved exception
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
    }
}
