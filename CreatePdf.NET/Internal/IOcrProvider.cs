namespace CreatePdf.NET.Internal;

/// <summary>
///     Defines the contract for an OCR provider capable of rasterizing PDF pages and extracting text from images.
/// </summary>
internal interface IOcrProvider
{
    /// <summary>
    ///     Rasterizes a PDF page to a PNG image.
    /// </summary>
    /// <param name="pdfPath">The file path of the source PDF.</param>
    /// <param name="pngPath">The file path where the resulting PNG should be saved.</param>
    /// <param name="options">Configuration options for the OCR process (e.g., DPI).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RasterizePdfToPngAsync(string pdfPath, string pngPath, OcrOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Extracts text from an image file using OCR.
    /// </summary>
    /// <param name="pngPath">The file path of the source image.</param>
    /// <param name="txtPath">The file path where the extracted text should be saved.</param>
    /// <param name="options">Configuration options for the OCR process (e.g., language, PSM).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing the extracted text.</returns>
    Task<string> ExtractTextFromImageAsync(string pngPath, string txtPath, OcrOptions options,
        CancellationToken cancellationToken = default);
}
