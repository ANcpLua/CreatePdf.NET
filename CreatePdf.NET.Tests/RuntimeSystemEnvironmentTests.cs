using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class RuntimeSystemEnvironmentTests
{
    [Fact]
    public void Properties_MirrorUnderlyingEnvironment()
    {
        var env = RuntimeSystemEnvironment.Instance;

        env.IsMacOS.Should().Be(OperatingSystem.IsMacOS());
        env.IsWindows.Should().Be(OperatingSystem.IsWindows());
        env.Is64BitOperatingSystem.Should().Be(Environment.Is64BitOperatingSystem);
    }

    [Fact]
    public void FileExists_DelegatesToFileSystem()
    {
        var temp = Path.GetTempFileName();
        try
        {
            RuntimeSystemEnvironment.Instance.FileExists(temp).Should().BeTrue();
        }
        finally
        {
            File.Delete(temp);
        }

        RuntimeSystemEnvironment.Instance.FileExists(temp).Should().BeFalse();
    }
}
