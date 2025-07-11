using System.Buffers;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Internal;
using CreatePdf.NET.Public;

namespace CreatePdf.NET.Tests;

public class BitmapImageTests
{
    [Fact]
    public void FromBitmap_WithSimpleBitmap_CreatesCorrectPixelData()
    {
        var bitmap = new[] { true, false, false, true };
        var foreground = Dye.Red;
        var background = Dye.Blue;
        const int width = 2;
        const int height = 2;

        using var image = BitmapImage.FromBitmap(bitmap, width, height, foreground, background);

        var pixels = image.GetPixelMemory().ToArray();
        pixels.Length.Should().Be(12);

        pixels.Should().Equal(255, 0, 0, 0, 0, 255, 0, 0, 255, 255, 0, 0);
    }

    [Fact]
    public void FromBitmap_WithSimilarColors_AppliesAutoContrast()
    {
        var bitmap = new[] { true };
        var similarForeground = new Dye(0.5f, 0.5f, 0.5f);
        var similarBackground = new Dye(0.55f, 0.55f, 0.55f);

        using var image = BitmapImage.FromBitmap(bitmap, 1, 1, similarForeground, similarBackground);

        image.GetPixelMemory().ToArray()
            .Should().Equal(0, 0, 0);
    }

    [Fact]
    public void GetPixelMemory_AfterDispose_ThrowsObjectDisposedException()
    {
        var bitmap = new[] { true };
        var image = BitmapImage.FromBitmap(bitmap, 1, 1, Dye.Red, Dye.Blue);
        image.Dispose();

        Action act = () => image.GetPixelMemory();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void FromBitmap_WithEmptyBitmap_CreatesBackgroundOnlyImage()
    {
        var bitmap = new[] { false, false, false, false };
        var background = Dye.Green;

        using var image = BitmapImage.FromBitmap(bitmap, 2, 2, Dye.Red, background);

        var expectedGreenPixels = new byte[]
        {
            0, 255, 0,
            0, 255, 0,
            0, 255, 0,
            0, 255, 0
        };

        image.GetPixelMemory().ToArray()
            .Should().Equal(expectedGreenPixels);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        const int width = 100;
        const int height = 50;
        var memory = MemoryPool<byte>.Shared.Rent(width * height * 3);

        using var image = new BitmapImage(width, height, memory);

        using (new AssertionScope())
        {
            image.Width.Should().Be(100);
            image.Height.Should().Be(50);
            image.Size.Should().Be(15000);
        }
    }

    [Fact]
    public void FromBitmap_WithAllTruePixels_CreatesUniformForegroundImage()
    {
        var bitmap = Enumerable.Repeat(true, 100).ToArray();
        var foreground = Dye.Yellow;

        using var image = BitmapImage.FromBitmap(bitmap, 10, 10, foreground, Dye.Black);

        var pixels = image.GetPixelMemory().ToArray();

        for (var i = 0; i < pixels.Length; i += 3)
        {
            pixels[i].Should().Be(255);
            pixels[i + 1].Should().Be(255);
            pixels[i + 2].Should().Be(0);
        }
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var bitmap = new[] { true };
        var image = BitmapImage.FromBitmap(bitmap, 1, 1, Dye.Red, Dye.Blue);

        var act = () =>
        {
            image.Dispose();
            image.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void FromBitmap_WithPatternedBitmap_CreatesCorrectPattern()
    {
        var bitmap = new[] { true, false, false, true };

        using var image = BitmapImage.FromBitmap(bitmap, 2, 2, Dye.White, Dye.Black);

        using (new AssertionScope())
        {
            image.Width.Should().Be(2);
            image.Height.Should().Be(2);
            image.Size.Should().Be(12);

            var pixels = image.GetPixelMemory().ToArray();

            pixels.Take(3).Should().Equal(255, 255, 255);

            pixels.Skip(9).Take(3).Should().Equal(255, 255, 255);
        }
    }

    [Fact]
    public void BitmapImage_Properties_AreConsistent()
    {
        using var image = BitmapImage.FromBitmap([true], 1, 1, Dye.Red, Dye.Blue);

        using (new AssertionScope())
        {
            image.Width.Should().BeGreaterThan(0);
            image.Height.Should().BeGreaterThan(0);
            image.Size.Should().Be(image.Width * image.Height * 3);
            image.GetPixelMemory().ToArray().Length.Should().Be(image.Size);
        }
    }
}