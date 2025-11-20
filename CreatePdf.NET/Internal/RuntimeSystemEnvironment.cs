namespace CreatePdf.NET.Internal;

internal sealed class RuntimeSystemEnvironment : ISystemEnvironment
{
    public static RuntimeSystemEnvironment Instance { get; } = new();

    public bool IsMacOS => OperatingSystem.IsMacOS();

    public bool IsWindows => OperatingSystem.IsWindows();

    public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;

    public bool FileExists(string path) => File.Exists(path);
}
