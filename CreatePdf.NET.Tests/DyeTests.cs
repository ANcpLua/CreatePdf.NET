using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CreatePdf.NET.Public;

namespace CreatePdf.NET.Tests;

public class DyeTests
{
    [Fact]
    public void Constructor_ShouldCreateColorWithSpecifiedValues()
    {
        var color = new Dye(0.5f, 0.1f, 0.2f);

        using (new AssertionScope())
        {
            color.R.Should().Be(0.5f);
            color.G.Should().Be(0.1f);
            color.B.Should().Be(0.2f);
        }
    }

    [Fact]
    public void PredefinedColors_ShouldHaveCorrectValues()
    {
        using (new AssertionScope())
        {
            Dye.White.Should().Be(new Dye(1, 1, 1));
            Dye.Black.Should().Be(new Dye(0, 0, 0));
            Dye.Red.Should().Be(new Dye(1, 0, 0));
            Dye.Green.Should().Be(new Dye(0, 1, 0));
            Dye.Blue.Should().Be(new Dye(0, 0, 1));
            Dye.Gray.Should().Be(new Dye(0.5f, 0.5f, 0.5f));
            Dye.Orange.Should().Be(new Dye(1f, 0.65f, 0f));
        }
    }

    [Fact]
    public void StaticColors_HaveCorrectRgbValues()
    {
        using (new AssertionScope())
        {
            Dye.White.Should().BeEquivalentTo(new { R = 1f, G = 1f, B = 1f });
            Dye.Black.Should().BeEquivalentTo(new { R = 0f, G = 0f, B = 0f });
            Dye.Red.Should().BeEquivalentTo(new { R = 1f, G = 0f, B = 0f });
            Dye.Green.Should().BeEquivalentTo(new { R = 0f, G = 1f, B = 0f });
            Dye.Blue.Should().BeEquivalentTo(new { R = 0f, G = 0f, B = 1f });
        }
    }

    [Theory]
    [InlineData(1f, 1f, 1f, 1f)]
    [InlineData(0f, 0f, 0f, 0f)]
    [InlineData(0.5f, 0.5f, 0.5f, 0.5f)]
    public void Luminance_CalculatesCorrectly(float r, float g, float b, float expected)
    {
        var dye = new Dye(r, g, b);

        dye.Luminance.Should().BeApproximately(expected, 0.01f);
    }

    [Theory]
    [InlineData(0.6f, true)]
    [InlineData(0.4f, false)]
    public void IsLight_DeterminesCorrectly(float luminance, bool expectedIsLight)
    {
        var dye = new Dye(luminance, luminance, luminance);

        dye.IsLight.Should().Be(expectedIsLight);
    }

    [Fact]
    public void IsSimilarTo_WithinTolerance_ReturnsTrue()
    {
        var dye1 = new Dye(0.5f, 0.5f, 0.5f);
        var dye2 = new Dye(0.55f, 0.45f, 0.52f);

        dye1.IsSimilarTo(dye2).Should().BeTrue();
    }

    [Fact]
    public void IsSimilarTo_OutsideTolerance_ReturnsFalse()
    {
        var dye1 = new Dye(0.5f, 0.5f, 0.5f);
        var dye2 = new Dye(0.7f, 0.5f, 0.5f);

        dye1.IsSimilarTo(dye2).Should().BeFalse();
    }

    [Fact]
    public void IsSimilarTo_DefaultTolerance_Works()
    {
        var dye1 = Dye.Gray;
        var dye2 = new Dye(0.55f, 0.55f, 0.55f);

        dye1.IsSimilarTo(dye2).Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_WorksCorrectly()
    {
        var dye1 = new Dye(0.5f, 0.5f, 0.5f);
        var dye2 = new Dye(0.5f, 0.5f, 0.5f);
        var dye3 = new Dye(0.5f, 0.5f, 0.6f);

        dye1.Should().Be(dye2);
        dye1.Should().NotBe(dye3);
    }

    [Fact]
    public void NamedColors_HaveExpectedProperties()
    {
        using (new AssertionScope())
        {
            Dye.White.IsLight.Should().BeTrue();
            Dye.Black.IsLight.Should().BeFalse();
            Dye.Yellow.IsLight.Should().BeTrue();
            Dye.DarkGray.IsLight.Should().BeFalse();
        }
    }
}