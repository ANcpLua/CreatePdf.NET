namespace CreatePdf.NET.Public;

/// <summary>
/// Provides factory methods for creating PDF documents.
/// </summary>
public static class Pdf
{
    /// <summary>
    /// Creates a new PDF document with optional background color.
    /// </summary>
    /// <param name="backgroundColor">Background color for all pages. Defaults to white.</param>
    /// <returns>A new <see cref="Document"/> instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// await Pdf.Create(Dye.Pink)
    ///     .AddText("Hello World")
    ///     .SaveAsync("output.pdf");
    /// </code>
    /// </example>
    public static Document Create(Dye? backgroundColor = null)
    {
        return new Document(backgroundColor ?? Dye.White);
    }
}