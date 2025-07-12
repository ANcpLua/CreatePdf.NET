using CreatePdf.NET.Internal;

namespace CreatePdf.NET;

/// <summary>
/// Represents a PDF reader for processing external PDF content.
/// </summary>
public sealed class PdfReader
{
    private readonly Stream _pdfStream;

    internal PdfReader(Stream pdfStream)
    {
        _pdfStream = pdfStream;
    }

    /// <summary>
    /// Performs OCR on the PDF and extracts text.
    /// </summary>
    /// <param name="options">Optional OCR settings. If not provided, uses defaults.</param>
    /// <returns>The extracted text from the PDF.</returns>
    public async Task<string> OcrAsync(OcrOptions? options = null)
    {
        return await OcrService.ProcessPdfStreamAsync(_pdfStream, options ?? new OcrOptions());
    }
}