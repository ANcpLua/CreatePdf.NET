using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET;

/// <summary>
/// Represents an RGB color for use in PDF documents.
/// </summary>
/// <param name="R">Red component (0.0 to 1.0).</param>
/// <param name="G">Green component (0.0 to 1.0).</param>
/// <param name="B">Blue component (0.0 to 1.0).</param>
[ExcludeFromCodeCoverage]
public readonly record struct Dye(float R, float G, float B)
{
    /// <summary>White (1, 1, 1).</summary>
    public static Dye White => new(1, 1, 1);
    
    /// <summary>Black (0, 0, 0).</summary>
    public static Dye Black => new(0, 0, 0);
    
    /// <summary>Red (1, 0, 0).</summary>
    public static Dye Red => new(1, 0, 0);
    
    /// <summary>Green (0, 1, 0).</summary>
    public static Dye Green => new(0, 1, 0);
    
    /// <summary>Blue (0, 0, 1).</summary>
    public static Dye Blue => new(0, 0, 1);
    
    /// <summary>Gray (0.5, 0.5, 0.5).</summary>
    public static Dye Gray => new(0.5f, 0.5f, 0.5f);
    
    /// <summary>Dark gray (0.3, 0.3, 0.3).</summary>
    public static Dye DarkGray => new(0.3f, 0.3f, 0.3f);
    
    /// <summary>Orange (1, 0.65, 0).</summary>
    public static Dye Orange => new(1f, 0.65f, 0f);
    
    /// <summary>Yellow (1, 1, 0).</summary>
    public static Dye Yellow => new(1f, 1f, 0f);
    
    /// <summary>Purple (0.5, 0, 0.5).</summary>
    public static Dye Purple => new(0.5f, 0f, 0.5f);
    
    /// <summary>Pink (1, 0.75, 0.8).</summary>
    public static Dye Pink => new(1f, 0.75f, 0.8f);
    
    /// <summary>Cyan (0, 1, 1).</summary>
    public static Dye Cyan => new(0f, 1f, 1f);
    
    /// <summary>Brown (0.6, 0.4, 0.2).</summary>
    public static Dye Brown => new(0.6f, 0.4f, 0.2f);

    /// <summary>
    /// Gets the perceived brightness using ITU-R BT.601 luminance coefficients.
    /// </summary>
    internal float Luminance => R * 0.299f + G * 0.587f + B * 0.114f;

    /// <summary>
    /// Gets a value indicating whether this color is considered light (luminance > 0.5).
    /// </summary>
    public bool IsLight => Luminance > 0.5f;

    /// <summary>
    /// Determines whether this color is similar to another color within a tolerance.
    /// </summary>
    /// <param name="other">The color to compare with.</param>
    /// <param name="tolerance">Maximum difference per component (default: 0.1).</param>
    /// <returns><c>true</c> if colors are similar; otherwise, <c>false</c>.</returns>
    public bool IsSimilarTo(Dye other, float tolerance = 0.1f)
    {
        return Math.Abs(R - other.R) < tolerance &&
               Math.Abs(G - other.G) < tolerance &&
               Math.Abs(B - other.B) < tolerance;
    }
}