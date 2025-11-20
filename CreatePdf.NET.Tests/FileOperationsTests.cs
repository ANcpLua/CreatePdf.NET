using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class FileOperationsTests
{
    [Theory]
    // Basic filenames
    [InlineData("test", "test.pdf")]
    [InlineData("test.pdf", "test.pdf")]
    [InlineData("test.PDF", "test.pdf")]
    [InlineData("test.txt", "test.pdf")]
    // Invalid characters
    [InlineData("file:name", "file_name.pdf")]
    [InlineData("file|name", "file_name.pdf")]
    [InlineData("file<>name", "file__name.pdf")]
    [InlineData("file*name", "file_name.pdf")]
    [InlineData("file?name", "file_name.pdf")]
    [InlineData("file\"name", "file_name.pdf")]
    // Path separators
    [InlineData("file/name", "name.pdf")]
    [InlineData("file\\name", "file_name.pdf")]
    [InlineData("path/to/file", "file.pdf")]
    [InlineData(@"path\to\file", "path_to_file.pdf")]
    // Path traversal
    [InlineData("../../../etc/passwd", "passwd.pdf")]
    [InlineData("../../../../root/.ssh/id_rsa", "id_rsa.pdf")]
    [InlineData("../output.pdf", "output.pdf")]
    [InlineData("/etc/passwd", "passwd.pdf")]
    [InlineData(@"..\..\file.pdf", "file.pdf")]
    [InlineData(@"C:\Windows\System32\file.pdf", "C__Windows_System32_file.pdf")]
    [InlineData(@"\\server\share\file", "server_share_file.pdf")]
    // Control characters
    [InlineData("file\0name", "file_name.pdf")]
    [InlineData("file\nname", "file_name.pdf")]
    [InlineData("file\rname", "file_name.pdf")]
    [InlineData("file\tname", "file_name.pdf")]
    // Windows reserved names
    [InlineData("CON", "CON.pdf")]
    [InlineData("PRN.txt", "PRN.pdf")]
    [InlineData("AUX.docx", "AUX.pdf")]
    [InlineData("NUL", "NUL.pdf")]
    [InlineData("COM1", "COM1.pdf")]
    [InlineData("LPT1", "LPT1.pdf")]
    // Multiple extensions
    [InlineData("file.pdf.exe", "file.pdf.pdf")]
    [InlineData("file.txt.pdf", "file.txt.pdf")]
    // Allowed special characters
    [InlineData("invoice_2025-01-11.txt", "invoice_2025-01-11.pdf")]
    [InlineData("file (copy).txt", "file (copy).pdf")]
    [InlineData("user@domain.txt", "user@domain.pdf")]
    [InlineData("price_$99.99.txt", "price_$99.99.pdf")]
    // Multiple invalid chars
    [InlineData("file:with|many<invalid>chars*?.txt", "file_with_many_invalid_chars__.pdf")]
    // Whitespace
    [InlineData("  file  ", "file.pdf")]
    [InlineData("\tfile\t", "file.pdf")]
    // Unicode
    [InlineData("café.txt", "café.pdf")]
    [InlineData("文档.txt", "文档.pdf")]
    [InlineData("файл.txt", "файл.pdf")]
    [InlineData("αρχείο.txt", "αρχείο.pdf")]
    [InlineData("emoji_😀.txt", "emoji_😀.pdf")]
    // Mixed valid/invalid
    [InlineData("test\r\nfile", "test__file.pdf")]
    [InlineData("valid\0invalid", "valid_invalid.pdf")]
    public void GetOutputPath_TransformsInputToValidPdfFilename(string input, string expectedFilename)
    {
        var result = FileOperations.GetOutputPath(input);

        Path.GetFileName(result).Should().Be(expectedFilename);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(@"/\/\/\")]
    [InlineData("<<<>>>")]
    [InlineData("***???")]
    [InlineData("   ")]
    [InlineData("\t\t\t")]
    public void GetOutputPath_WithInvalidOrEmptyInput_GeneratesTimestampedFilename(string? input)
    {
        var result = FileOperations.GetOutputPath(input);

        Path.GetFileName(result).Should().StartWith("document_").And.MatchRegex(@"document_\d{13}\.pdf");
    }

    [Fact]
    public void GetOutputPath_Always_ReturnsFullyQualifiedPath()
    {
        var result = FileOperations.GetOutputPath("test.pdf");

        Path.IsPathFullyQualified(result).Should().BeTrue();
    }

    [Fact]
    public void GetOutputPath_Always_CreatesOutputDirectory()
    {
        var result = FileOperations.GetOutputPath("test.pdf");
        var directory = Path.GetDirectoryName(result);

        directory.Should().NotBeNull();
        Directory.Exists(directory).Should().BeTrue();
    }

    [Fact]
    public void GetOutputPath_WithLongFilename_PreservesFullName()
    {
        var longName = new string('a', 200) + ".txt";
        var expectedName = new string('a', 200) + ".pdf";

        var result = FileOperations.GetOutputPath(longName);

        Path.GetFileName(result).Should().Be(expectedName);
    }

    [Fact]
    public void GetOutputPath_FromDeepBinDirectory_CreatesOutputInProjectRoot()
    {
        var result = FileOperations.GetOutputPath("test.pdf");

        result.Should().Contain("output");

        result.Should().NotContain("bin/Debug");
        result.Should().NotContain("bin/Release");

        var outputDir = Path.GetDirectoryName(result);
        Directory.Exists(outputDir).Should().BeTrue();

        var pathSegments = result.Split(Path.DirectorySeparatorChar);
        pathSegments.Should().NotContain("net8.0");
        pathSegments.Should().NotContain("net9.0");
        pathSegments.Should().NotContain("net10.0");
    }

    [Fact]
    public void FindProjectRoot_WhenNoProjectFiles_ReturnsNull()
    {
        var result = FileOperations.FindProjectRoot(Path.GetTempPath());

        result.Should().BeNull();
    }
}
