using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET.Internal;

internal static class FileOperations
{
    public static readonly SearchValues<char> InvalidFileChars = SearchValues.Create([
        ..Path.GetInvalidFileNameChars(),
        ':', '|', '<', '>', '"', '*', '?', '\\', '/'
    ]);

    [ExcludeFromCodeCoverage]
    public static void Open(string path)
    {
        var (cmd, args) = OperatingSystem.IsWindows() ? ("cmd", $"/c start \"\" \"{path}\"") :
            OperatingSystem.IsMacOS() ? ("open", path) :
            ("xdg-open", path);

        Process.Start(new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }

    [ExcludeFromCodeCoverage]
    public static void ShowDirectory(string path)
    {
        var (cmd, args) = OperatingSystem.IsWindows() ? ("explorer", $"/select,\"{path}\"") :
            OperatingSystem.IsMacOS() ? ("open", $"-R {path}") :
            ("xdg-open", Path.GetDirectoryName(path)!);

        Process.Start(new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            UseShellExecute = true,
            CreateNoWindow = true
        });
    }

    public static string GetOutputPath(string? filename)
    {
        var outputDir = GetUserFriendlyDirectory("output");
        var name = BuildSafeFileName(filename);
        return Path.Combine(outputDir, name);
    }

    internal static string GetUserFriendlyDirectory(string subdir)
    {
        var projectRoot = FindProjectRoot();
        return projectRoot != null
            ? Directory.CreateDirectory(Path.Combine(projectRoot, subdir)).FullName
            : Directory.CreateDirectory(subdir).FullName;
    }

    private static string? FindProjectRoot()
    {
        var current = Directory.GetCurrentDirectory();
        for (var dir = new DirectoryInfo(current); dir != null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.csproj").Any() || dir.EnumerateFiles("*.sln").Any())
                return dir.FullName;
        }

        return null;
    }

    private static string BuildSafeFileName(string? filename)
    {
        if (string.IsNullOrEmpty(filename))
            return $"document_{DateTime.Now:yyyyMMddHHmmss}.pdf";

        var sanitized = filename.AsSpan().IndexOfAny(InvalidFileChars) >= 0
            ? string.Create(filename.Length, filename, (chars, state) =>
            {
                state.AsSpan().CopyTo(chars);
                chars.ReplaceAny(InvalidFileChars, '_');
            })
            : filename;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
        return $"{nameWithoutExt}.pdf";
    }
}