using System.Diagnostics.CodeAnalysis;

namespace CreatePdf.NET.Public;

/// <summary>
/// Defines standard text sizes for PDF documents.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly record struct TextSize(int Value)
{
    /// <summary>Small text (12pt).</summary>
    public static readonly TextSize Small = new(12);

    /// <summary>Medium text (18pt).</summary>
    public static readonly TextSize Medium = new(18);

    /// <summary>Large text (24pt).</summary>
    public static readonly TextSize Large = new(24);
}