namespace CreatePdf.NET.Public;

/// <summary>
/// Configuration options for OCR processing.
/// </summary>
public sealed class OcrOptions
{
    /// <summary>
    /// DPI for PDF to image conversion (default: 300).
    /// </summary>
    public int Dpi { get; set; } = 300;
    
    /// <summary>
    /// OCR language (default: "eng").
    /// </summary>
    public string Language { get; set; } = "eng";
    
    /// <summary>
    /// Tesseract page segmentation mode (default: 6).
    /// </summary>
    public int PageSegmentationMode { get; set; } = 6;
    
    /// <summary>
    /// Output directory for OCR files (default: "ocr").
    /// </summary>
    public string OutputDirectory { get; set; } = "ocr";
    
    /// <summary>
    /// Custom Tesseract executable path (optional).
    /// </summary>
    public string? TesseractPath { get; set; }
    
    /// <summary>
    /// Custom PDF converter path (optional).
    /// </summary>
    public string? PdfConverterPath { get; set; }
}