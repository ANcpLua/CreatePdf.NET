using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET.Tests;

public class FileOperationsTests
{
    [Fact]
    public void InvalidFileChars_ContainsExpectedCharacters()
    {
        var expectedChars = new[] { ':', '|', '<', '>', '"', '*', '?', '\\', '/' };

        expectedChars.Should().OnlyContain(ch => FileOperations.InvalidFileChars.Contains(ch));
    }

    [Theory]
    [InlineData(null, "document_")]
    [InlineData("", "document_")]
    [InlineData("test", "test.pdf")]
    [InlineData("test.pdf", "test.pdf")]
    [InlineData("test.PDF", "test.pdf")]
    [InlineData("test.txt", "test.pdf")]
    [InlineData("file:name", "file_name.pdf")]
    [InlineData("file|name", "file_name.pdf")]
    [InlineData("file<>name", "file__name.pdf")]
    [InlineData("file\\name", "file_name.pdf")]
    [InlineData("file/name", "file_name.pdf")]
    public void GetOutputPath_HandlesVariousInputs(string? input, string expectedPrefix)
    {
        var result = FileOperations.GetOutputPath(input);

        result.Should().EndWith(".pdf");
        Path.GetFileName(result).Should().StartWith(expectedPrefix);
    }

    [Fact]
    public void GetOutputPath_WithInvalidChars_ReplacesWithUnderscore()
    {
        const string invalidName = "file:with|many<invalid>chars*?.txt";

        var result = FileOperations.GetOutputPath(invalidName);

        var filename = Path.GetFileName(result);
        filename.Should().Be("file_with_many_invalid_chars__.pdf");
    }

    [Fact]
    public void GetOutputPath_GeneratesTimestampedName_WhenNoFilenameProvided()
    {
        var result1 = FileOperations.GetOutputPath(null);
        var result2 = FileOperations.GetOutputPath("");

        using (new AssertionScope())
        {
            Path.GetFileName(result1).Should().StartWith("document_");
            Path.GetFileName(result1).Should().MatchRegex(@"document_\d{14}\.pdf");
            Path.GetFileName(result2).Should().StartWith("document_");
        }
    }

    [Fact]
    public void GetUserFriendlyDirectory_CreatesDirectory()
    {
        var testDir = $"test_output_{Guid.NewGuid():N}";
        
        var createdDir = FileOperations.GetUserFriendlyDirectory(testDir);
        
        Directory.Exists(createdDir).Should().BeTrue();
        createdDir.Should().EndWith(testDir);
    }
    
    [Fact]
    public void FindProjectRoot_NoProjectFiles_FallsBackToCurrentDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"no_project_{Guid.NewGuid():N}");
        var nestedDir = Path.Combine(tempDir, "nested", "deep", "folder");
        Directory.CreateDirectory(nestedDir);
        Directory.SetCurrentDirectory(nestedDir);
        
        var result = FileOperations.GetUserFriendlyDirectory("test");
        
        Directory.Exists(result).Should().BeTrue();
        result.Should().EndWith("test");
    }

    [Fact]
    public void GetOutputPath_CreatesOutputDirectory()
    {
        var filename = $"test_{Guid.NewGuid():N}.pdf";
        
        var result = FileOperations.GetOutputPath(filename);
        
        var directory = Path.GetDirectoryName(result);
        directory.Should().NotBeNull();
        Directory.Exists(directory).Should().BeTrue();
        result.Should().EndWith(filename);
    }

    [Fact]
    public void GetOutputPath_ReturnsFullPath()
    {
        var result = FileOperations.GetOutputPath("test.pdf");
        
        Path.IsPathFullyQualified(result).Should().BeTrue();
    }
}