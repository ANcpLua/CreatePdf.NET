namespace CreatePdf.NET;

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

    /// <summary>
    /// Loads a PDF from a stream for reading and processing.
    /// </summary>
    /// <param name="pdfStream">The PDF content as a stream.</param>
    /// <returns>A PdfReader instance for performing operations on the PDF.</returns>
    /// <example>
    /// <code>
    /// await using var stream = new MemoryStream(pdfBytes);
    /// var text = await Pdf.Load(stream).OcrAsync();
    /// </code>
    /// </example>
    public static PdfReader Load(Stream pdfStream)
    {
        return new PdfReader(pdfStream);
    }
}