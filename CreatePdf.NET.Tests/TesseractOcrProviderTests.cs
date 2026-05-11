using System.Diagnostics;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class TesseractOcrProviderTests
{
    private const string AppleSiliconPath = "/opt/homebrew/bin/tesseract";
    private const string IntelMacPath = "/usr/local/bin/tesseract";

    [Fact]
    public async Task ExtractTextFromImageAsync_WhenOutputIsMissing_ThrowsFileNotFound()
    {
        var processRunner = new FakeProcessRunner();
        var environment = new FakeSystemEnvironment { FileExistsImpl = _ => false };
        var engine = new TesseractOcrProvider(environment, processRunner);

        var act = () => engine.ExtractTextFromImageAsync(
            "input.png",
            Path.Combine(Path.GetTempPath(), "missing-output.txt"),
            new OcrOptions { TesseractPath = "/bin/echo" });

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("OCR output file not found. Tesseract execution failed*")
            .ConfigureAwait(true);

        processRunner.StartInfos.Should().HaveCount(1);
        processRunner.StartInfos[0].FileName.Should().Be("/bin/echo");
        processRunner.StartInfos[0].Arguments.Should().Contain("input.png");
    }

    [Fact]
    public async Task ExtractTextFromImageAsync_WhenOutputPathHasNonTxtExtension_ChecksDerivedTxtPath()
    {
        // Tesseract always appends ".txt" to the base it is given. If the caller hands us
        // a path like "out.log", the provider must read back "out.txt", not "out.log".
        var checkedPaths = new List<string>();
        var processRunner = new FakeProcessRunner();
        var environment = new FakeSystemEnvironment
        {
            FileExistsImpl = path =>
            {
                checkedPaths.Add(path);
                return false;
            }
        };
        var engine = new TesseractOcrProvider(environment, processRunner);

        var tempDir = Path.GetTempPath();
        var nonTxtPath = Path.Combine(tempDir, "out.log");
        var expectedDerivedPath = Path.Combine(tempDir, "out.txt");

        var act = () => engine.ExtractTextFromImageAsync(
            "input.png",
            nonTxtPath,
            new OcrOptions { TesseractPath = "/bin/echo" });

        await act.Should().ThrowAsync<FileNotFoundException>()
            .Where(e => e.FileName == expectedDerivedPath)
            .ConfigureAwait(true);

        checkedPaths.Should().ContainSingle().Which.Should().Be(expectedDerivedPath);
    }

    [Fact]
    public void GetPdfRasterizerExecutable_UsesExplicitConverterPath()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment(), new FakeProcessRunner());
        var options = new OcrOptions { PdfConverterPath = "/custom/gs" };

        engine.GetPdfRasterizerExecutable(options).Should().Be("/custom/gs");
    }

    [Fact]
    public void GetPdfRasterizerExecutable_MacOs_ReturnsSips()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment { IsMacOS = true }, new FakeProcessRunner());

        engine.GetPdfRasterizerExecutable(new OcrOptions()).Should().Be("/usr/bin/sips");
    }

    [Theory]
    [InlineData(true, "gswin64c")]
    [InlineData(false, "gswin32c")]
    public void GetPdfRasterizerExecutable_Windows_SelectsBitnessSpecificCommand(bool is64Bit, string expected)
    {
        var environment = new FakeSystemEnvironment { IsWindows = true, Is64BitOperatingSystem = is64Bit };
        var engine = new TesseractOcrProvider(environment, new FakeProcessRunner());

        engine.GetPdfRasterizerExecutable(new OcrOptions()).Should().Be(expected);
    }

    [Fact]
    public void GetPdfRasterizerExecutable_DefaultsToUnixCommand()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment(), new FakeProcessRunner());

        engine.GetPdfRasterizerExecutable(new OcrOptions()).Should().Be("gs");
    }

    [Fact]
    public void GetTesseractExecutable_UsesProvidedPath()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment(), new FakeProcessRunner());
        var options = new OcrOptions { TesseractPath = "/custom/tesseract" };

        engine.GetTesseractExecutable(options).Should().Be("/custom/tesseract");
    }

    [Fact]
    public void GetTesseractExecutable_MacOsPrefersAppleSiliconBinary()
    {
        var environment = new FakeSystemEnvironment
        {
            IsMacOS = true,
            FileExistsImpl = path => string.Equals(path, AppleSiliconPath, StringComparison.Ordinal)
        };
        var engine = new TesseractOcrProvider(environment, new FakeProcessRunner());

        engine.GetTesseractExecutable(new OcrOptions()).Should().Be(AppleSiliconPath);
    }

    [Fact]
    public void GetTesseractExecutable_MacOsFallsBackToIntelBinary()
    {
        var environment = new FakeSystemEnvironment
        {
            IsMacOS = true,
            FileExistsImpl = path => string.Equals(path, IntelMacPath, StringComparison.Ordinal)
        };
        var engine = new TesseractOcrProvider(environment, new FakeProcessRunner());

        engine.GetTesseractExecutable(new OcrOptions()).Should().Be(IntelMacPath);
    }

    [Fact]
    public void GetTesseractExecutable_UsesFallbackWhenNoMacBinaryFound()
    {
        var engine = new TesseractOcrProvider(
            new FakeSystemEnvironment { IsMacOS = true },
            new FakeProcessRunner());

        engine.GetTesseractExecutable(new OcrOptions()).Should().Be("tesseract");
    }

    [Fact]
    public void GetRasterizationArguments_MacOs_UsesSipsFormat()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment { IsMacOS = true }, new FakeProcessRunner());

        engine.GetRasterizationArguments("file.pdf", "file.png", new OcrOptions { Dpi = 150 })
            .Should()
            .Be("-s format png -s dpiHeight 150 -s dpiWidth 150 \"file.pdf\" --out \"file.png\"");
    }

    [Fact]
    public void GetRasterizationArguments_NonMac_UsesGhostscriptFormat()
    {
        var engine = new TesseractOcrProvider(new FakeSystemEnvironment(), new FakeProcessRunner());

        engine.GetRasterizationArguments("file.pdf", "file.png", new OcrOptions { Dpi = 200 })
            .Should()
            .Be("-dNOPAUSE -dBATCH -sDEVICE=pngalpha -dFirstPage=1 -dLastPage=1 -r200 -sOutputFile=\"file.png\" \"file.pdf\" ");
    }

    private sealed class FakeSystemEnvironment : ISystemEnvironment
    {
        public bool IsMacOS { get; set; }

        public bool IsWindows { get; set; }

        public bool Is64BitOperatingSystem { get; set; }

        public Func<string, bool>? FileExistsImpl { get; set; }

        public bool FileExists(string path) => (FileExistsImpl ?? (_ => false)).Invoke(path);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<ProcessStartInfo> StartInfos { get; } = [];

        public Task RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            StartInfos.Add(startInfo);
            return Task.CompletedTask;
        }
    }
}
