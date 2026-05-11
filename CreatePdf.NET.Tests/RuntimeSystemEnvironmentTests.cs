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
        var tempDir = Directory.CreateTempSubdirectory("createpdf-runtime-test-");
        var temp = Path.Combine(tempDir.FullName, "probe.txt");
        File.WriteAllText(temp, string.Empty);
        try
        {
            RuntimeSystemEnvironment.Instance.FileExists(temp).Should().BeTrue();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }

        RuntimeSystemEnvironment.Instance.FileExists(temp).Should().BeFalse();
    }

    [Fact]
    public async Task ReadAllTextAsync_DelegatesToFileSystem()
    {
        var tempDir = Directory.CreateTempSubdirectory("createpdf-runtime-read-");
        try
        {
            var path = Path.Combine(tempDir.FullName, "payload.txt");
            await File.WriteAllTextAsync(path, "hello world").ConfigureAwait(true);

            var text = await RuntimeSystemEnvironment.Instance
                .ReadAllTextAsync(path, CancellationToken.None)
                .ConfigureAwait(true);

            text.Should().Be("hello world");
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
