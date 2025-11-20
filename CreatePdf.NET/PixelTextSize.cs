using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET;

/// <summary>
///     Defines scale factors for pixel-based text rendering.
/// </summary>
/// <param name="Value">The scale multiplier for pixel text rendering.</param>
[ExcludeFromCodeCoverage(Justification = "Data record")]
public readonly record struct PixelTextSize(int Value)
{
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
