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
        var processRunner = new FakeProcessRunner { NextResult = new ProcessResult(1, "", "") };
        var environment = new FakeSystemEnvironment { FileExistsImpl = _ => false };
        var engine = new TesseractOcrProvider(environment, processRunner);

        var act = () => engine.ExtractTextFromImageAsync(
            "input.png",
            Path.Combine(Path.GetTempPath(), "missing-output.txt"),
            new OcrOptions { TesseractPath = "/bin/echo" });

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("OCR output file not found (Tesseract exited with code 1)*")
            .ConfigureAwait(true);

        processRunner.StartInfos.Should().HaveCount(1);
        processRunner.StartInfos[0].FileName.Should().Be("/bin/echo");
        processRunner.StartInfos[0].Arguments.Should().Contain("input.png");
    }

    [Fact]
    public async Task ExtractTextFromImageAsync_WhenOutputIsMissing_IncludesStderrInExceptionMessage()
    {
        const string tesseractStderr = "Error opening data file /usr/share/tessdata/eng.traineddata";
        var processRunner = new FakeProcessRunner
        {
            NextResult = new ProcessResult(ExitCode: 1, StandardOutput: "", StandardError: tesseractStderr)
        };
        var environment = new FakeSystemEnvironment { FileExistsImpl = _ => false };
        var engine = new TesseractOcrProvider(environment, processRunner);

        var act = () => engine.ExtractTextFromImageAsync(
            "input.png",
            Path.Combine(Path.GetTempPath(), "missing-output.txt"),
            new OcrOptions { TesseractPath = "/bin/echo" });

        await act.Should().ThrowAsync<FileNotFoundException>()
            .Where(e => e.Message.Contains(tesseractStderr, StringComparison.Ordinal)
                        && e.Message.Contains("exited with code 1", StringComparison.Ordinal))
            .ConfigureAwait(true);
    }

    [Theory]
    [InlineData("out.log", "out.txt")]
    [InlineData("out", "out.txt")]
    [InlineData("foo.bar.log", "foo.bar.txt")]
    [InlineData("out.TXT", "out.txt")]
    [InlineData("out.", "out.txt")]
    public async Task ExtractTextFromImageAsync_DerivesActualTxtPathFromBase(string inputName, string expectedName)
    {
        var tempDir = Path.GetTempPath();
        var inputPath = Path.Combine(tempDir, inputName);
        var expectedPath = Path.Combine(tempDir, expectedName);
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

        var act = () => engine.ExtractTextFromImageAsync(
            "input.png",
            inputPath,
            new OcrOptions { TesseractPath = "/bin/echo" });

        await act.Should().ThrowAsync<FileNotFoundException>()
            .Where(e => e.FileName == expectedPath)
            .ConfigureAwait(true);

        checkedPaths.Should().ContainSingle().Which.Should().Be(expectedPath);
    }

    [Fact]
    public async Task ExtractTextFromImageAsync_WithNonTxtInputPath_ReadsFromDerivedTxtPath()
    {
        string? readPath = null;
        var processRunner = new FakeProcessRunner();
        var environment = new FakeSystemEnvironment
        {
            FileExistsImpl = _ => true,
            ReadAllTextImpl = p =>
            {
                readPath = p;
                return "Hello\nWorld";
            }
        };
        var engine = new TesseractOcrProvider(environment, processRunner);

        var inputPath = Path.Combine(Path.GetTempPath(), "out.log");
        var expectedReadPath = Path.Combine(Path.GetTempPath(), "out.txt");

        var result = await engine.ExtractTextFromImageAsync(
            "input.png",
            inputPath,
            new OcrOptions { TesseractPath = "/bin/echo" }).ConfigureAwait(true);

        readPath.Should().Be(expectedReadPath);
        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task ExtractTextFromImageAsync_ReadsOutputViaSystemEnvironment_AndNormalisesWhitespace()
    {
        string? readPath = null;
        var environment = new FakeSystemEnvironment
        {
            FileExistsImpl = _ => true,
            ReadAllTextImpl = p =>
            {
                readPath = p;
                return "  Line 1\nLine 2\rLine 3  ";
            }
        };
        var engine = new TesseractOcrProvider(environment, new FakeProcessRunner());

        var result = await engine.ExtractTextFromImageAsync(
            "input.png", "output.txt", new OcrOptions { TesseractPath = "/bin/echo" })
            .ConfigureAwait(true);

        readPath.Should().Be("output.txt", "the provider must read through the abstraction, not File directly");
        result.Should().Be("Line 1 Line 2 Line 3");
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

        public Func<string, string>? ReadAllTextImpl { get; set; }

        public bool FileExists(string path) => (FileExistsImpl ?? (_ => false)).Invoke(path);

        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken) =>
            Task.FromResult((ReadAllTextImpl ?? (_ => string.Empty)).Invoke(path));
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<ProcessStartInfo> StartInfos { get; } = [];

        public ProcessResult NextResult { get; set; } = new(ExitCode: 0, StandardOutput: "", StandardError: "");

        public Task<ProcessResult> RunAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
        {
            StartInfos.Add(startInfo);
            return Task.FromResult(NextResult);
        }
    }
}
