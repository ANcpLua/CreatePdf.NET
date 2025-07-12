using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET;

/// <summary>
/// Defines scale factors for pixel-based text rendering.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct PixelTextSize
{
    /// <summary>
    /// Gets the scale multiplier for pixel text rendering.
    /// </summary>
    /// <value>The scale factor (1-5) that determines the final pixel size.</value>
    public int Value { get; }

    private PixelTextSize(int value)
    {
        Value = value;
    }

    /// <summary>Tiny scale (1x).</summary>
    public static readonly PixelTextSize Tiny = new(1);

    /// <summary>Small scale (2x).</summary>
    public static readonly PixelTextSize Small = new(2);

    /// <summary>Medium scale (3x).</summary>
    public static readonly PixelTextSize Medium = new(3);

    /// <summary>Large scale (4x).</summary>
    public static readonly PixelTextSize Large = new(4);

    /// <summary>Extra large scale (5x).</summary>
    public static readonly PixelTextSize ExtraLarge = new(5);
}