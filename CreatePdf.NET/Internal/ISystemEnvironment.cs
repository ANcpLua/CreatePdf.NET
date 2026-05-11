namespace CreatePdf.NET.Internal;

internal interface ISystemEnvironment
{
    bool IsMacOS { get; }

    bool IsWindows { get; }

    bool Is64BitOperatingSystem { get; }

    bool FileExists(string path);

    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken);
}
