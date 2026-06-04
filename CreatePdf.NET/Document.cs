using System.Diagnostics.CodeAnalysis;
using System.Text;
using CreatePdf.NET.Internal;

namespace CreatePdf.NET;

/// <summary>
///     Represents a PDF document that can be built using a fluent API.
/// </summary>
public sealed class Document
{
    private const int MaxTextLength = 10_000;
    private const int MaxContentItems = 1_000;
    private readonly Dye _background;
    private readonly List<IContent> _contents = [];
    private OcrOptions? _ocrOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Document" /> class with the specified background color.
    /// </summary>
    /// <param name="background">The background color for the PDF document.</param>
    public Document(Dye background)
    {
        _background = background;
    }

    /// <summary>
    ///     Configures OCR options for this document.
    /// </summary>
    /// <param name="configure">Action to configure <see cref="OcrOptions" />.</param>
    /// <returns>The document instance for chaining.</returns>
    /// <remarks>
    ///     Common options:<br />
    ///     - Dpi: Resolution for PDF to image conversion (default: <see cref="OcrOptions.Dpi" />)<br />
    ///     - Language: OCR language code (default: <see cref="OcrOptions.Language" />)<br />
    ///     - PageSegmentationMode: How Tesseract analyzes the page (default: <see cref="OcrOptions.PageSegmentationMode" />)
    ///     <br />
    ///     • 3 = Fully automatic page segmentation<br />
    ///     • 6 = Uniform block of text (good for PDFs)<br />
    ///     • 7 = Single text line<br />
    ///     • 8 = Single word<br />
    ///     <br />
    ///     Advanced options (rarely needed):<br />
    ///     - TesseractPath: Only if Tesseract is not in standard location<br />
    ///     - PdfConverterPath: Only if Ghostscript/sips is not in standard location
    /// </remarks>
    /// <example>
    ///     <code>
    /// .WithOcrOptions(opt => {
    ///     opt.Dpi = 600;
    ///     opt.Language = "eng";
    ///     opt.PageSegmentationMode = 7;
    /// })
    /// </code>
    /// </example>
    public Document WithOcrOptions(Action<OcrOptions> configure)
    {
        _ocrOptions ??= new OcrOptions();
        configure(_ocrOptions);
        return this;
    }

    /// <summary>
    ///     Saves the document and performs OCR to extract text.
    /// </summary>
    /// <param name="filename">Optional filename. If not provided, generates a timestamped name.</param>
    /// <returns>A tuple containing the file path and extracted text.</returns>
    /// <remarks>
    ///     Requires Tesseract and Ghostscript/sips to be installed.<br />
    ///     Configure OCR settings using <see cref="WithOcrOptions" /> before calling this method.<br />
    ///     Temporary OCR files are automatically cleaned up after processing.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var (path, text) = await document
    ///     .WithOcrOptions(opt => opt.Dpi = 600)
    ///     .SaveAndOcrAsync("report.pdf");
    /// </code>
    /// </example>
    public async Task<(string path, string text)> SaveAndOcrAsync(string? filename = null)
    {
        var path = await SaveAsync(filename);
        var text = await new OcrService().ProcessPdfAsync(path, _ocrOptions ?? new OcrOptions());
        return (path, text);
    }

    /// <summary>
    ///     Adds text to the document with specified formatting.
    /// </summary>
    /// <param name="text">The text to add.</param>
    /// <param name="color">Text color (default: <see cref="Dye.Black" />).</param>
    /// <param name="size">Text size (default: <see cref="TextSize.Medium" />).</param>
    /// <param name="alignment">Text alignment (default: <see cref="TextAlignment.Center" />).</param>
    /// <returns>The document instance for chaining.</returns>
    /// <example>
    ///     <code>
    /// .AddText("Hi", color: Dye.Blue, size: TextSize.Large, alignment: TextAlignment.Left);
    /// </code>
    /// </example>
    public Document AddText(
        string text,
        Dye? color = null,
        TextSize? size = null,
        TextAlignment? alignment = null)
    {
        ValidateContentLimit();
        var sanitized = SanitizeText(text);

        _contents.Add(new TextContent(
            sanitized,
            (size ?? TextSize.Medium).Value,
            color ?? Dye.Black,
            alignment ?? TextAlignment.Center
        ));

        return this;
    }

    /// <summary>
    ///     Adds text as a bitmap image with pixel-perfect rendering.
    /// </summary>
    /// <param name="text">The text to render as pixels.</param>
    /// <param name="textColor">Text color (default: <see cref="Dye.Black" />).</param>
    /// <param name="backgroundColor">Background color (default: <see cref="Dye.White" />).</param>
    /// <param name="size">Pixel text size (default: <see cref="PixelTextSize.Large" />).</param>
    /// <returns>The document instance for chaining.</returns>
    /// <example>
    ///     <code>
    /// .AddPixelText("Retro Text", textColor: Dye.Green, backgroundColor: Dye.Black, size: PixelTextSize.Medium);
    /// </code>
    /// </example>
    public Document AddPixelText(
        string text,
        Dye? textColor = null,
        Dye? backgroundColor = null,
        PixelTextSize? size = null)
    {
        ValidateContentLimit();
        var sanitized = SanitizeText(text);

        _contents.Add(new BitmapTextContent(
            sanitized,
            textColor ?? Dye.Black,
            backgroundColor ?? Dye.White,
            (size ?? PixelTextSize.Large).Value
        ));

        return this;
    }

    /// <summary>
    ///     Adds a blank line to the document.
    /// </summary>
    /// <returns>The document instance for chaining.</returns>
    public Document AddLine()
    {
        ValidateContentLimit();
        _contents.Add(new TextContent("", TextSize.Medium.Value, Dye.Black, TextAlignment.Left));
        return this;
    }

    /// <summary>
    ///     Adds multiple blank lines to the document.
    /// </summary>
    /// <param name="count">Number of lines to add (max 50).</param>
    /// <returns>The document instance for chaining.</returns>
    public Document AddLines(int count)
    {
        count = Math.Clamp(count, 0, 50);
        for (var i = 0; i < count; i++)
        {
            if (_contents.Count >= MaxContentItems)
                break;
            _ = AddLine();
        }

        return this;
    }

    /// <summary>
    ///     Saves the document and opens it in the default PDF viewer.
    /// </summary>
    /// <param name="filename">Optional filename. If not provided, generates a timestamped name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [ExcludeFromCodeCoverage(Justification = "Interacts with UI/Shell")]
    public async Task SaveAndOpenAsync(string? filename = null)
    {
        var path = await SaveAsync(filename);
        FileOperations.Open(path);
    }

    /// <summary>
    ///     Saves the document and shows its directory in the file explorer.
    /// </summary>
    /// <param name="filename">Optional filename. If not provided, generates a timestamped name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [ExcludeFromCodeCoverage(Justification = "Interacts with UI/Shell")]
    public async Task SaveAndShowDirectoryAsync(string? filename = null)
    {
        var path = await SaveAsync(filename);
        FileOperations.ShowDirectory(path);
    }

    /// <summary>
    ///     Renders the document to an in-memory PDF and returns its bytes, without touching the disk.
    /// </summary>
    /// <returns>The complete PDF document as a byte array.</returns>
    /// <example>
    ///     <code>
    /// byte[] pdf = await Pdf.Create(Dye.White)
    ///     .AddText("Hello World")
    ///     .ToBytesAsync();
    /// </code>
    /// </example>
    public async Task<byte[]> ToBytesAsync()
    {
        await using var stream = new MemoryStream();
        await using var writer = new PdfWriter(stream, _background);

        foreach (var content in _contents)
            content.Render(writer);

        await writer.FinalizeAsync();
        return stream.ToArray();
    }

    /// <summary>
    ///     Saves the document to a PDF file.
    /// </summary>
    /// <param name="filename">Optional filename. If not provided, generates a timestamped name.</param>
    /// <returns>The full path to the saved PDF file.</returns>
    public async Task<string> SaveAsync(string? filename = null)
    {
        var path = FileOperations.GetOutputPath(filename);
        await File.WriteAllBytesAsync(path, await ToBytesAsync());
        return path;
    }

    private void ValidateContentLimit()
    {
        if (_contents.Count >= MaxContentItems)
            throw new InvalidOperationException($"Document exceeds maximum content limit ({MaxContentItems} items)");
    }

    private static string SanitizeText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var input = text.Length > MaxTextLength ? text[..MaxTextLength] : text;
        var lines = input.Split('\n', 100, StringSplitOptions.TrimEntries);

        var sb = new StringBuilder(input.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                sb.Append('\n');
            AppendSanitizedLine(sb, lines[i]);
        }

        return sb.ToString();
    }

    private static void AppendSanitizedLine(StringBuilder sb, string line)
    {
        foreach (var c in line)
        {
            if (c == '\t')
                sb.Append(' ');
            else if (!char.IsControl(c))
                sb.Append(c);
        }
    }
}
